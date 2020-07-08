using System;
using System.IO;
using System.Threading.Tasks;
using app_api.Data;
using app_api.V1.Repos.UserRepo;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace app_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    // Any origin is allowed
                    builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((host) => true).AllowCredentials();
                });
            });
            services.AddControllers();

            Console.ForegroundColor = ConsoleColor.Red;
            // adding Database
            var connectionString = Configuration["KERBEROS_POSTGRES_CONNECTION_STRING"];
            if (connectionString == null)
            {
                Console.WriteLine("'KERBEROS_POSTGRES_CONNECTION_STRING' Database Connection string not found");
                System.Environment.Exit(1);
            }
            services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString, options => options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            )));
            Console.ResetColor();

            services.AddScoped<IUserRepo, UserRepo>();

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            // TODO
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                Console.WriteLine("Running in development mode");
                app.UseDeveloperExceptionPage();
            }
            await UpdateDatabase(app, logger);
            app.UseCors();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            await ExistingPrincipalsDatabaseCheck(app);

        }
        private async Task UpdateDatabase(IApplicationBuilder app, ILogger<Startup> logger)
        {
            using(var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using(var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    // Npgsql resiliency strategy does not work with Database.EnsureCreated() and Database.Migrate().
                    // Therefore a retry pattern is implemented for this purpose 
                    // if database connection is not ready it will retry 3 times before finally quiting
                    var retryCount = 3;
                    var currentRetry = 0;
                    while (true)
                    {
                        try
                        {
                            logger.LogInformation("Attempting database migration");

                            context.Database.Migrate();

                            logger.LogInformation("Database migration & connection successful");

                            break; // just break if migration is successful
                        }
                        catch (Npgsql.NpgsqlException)
                        {
                            logger.LogError("Database migration failed. Retrying in 5 seconds ...");

                            currentRetry++;

                            if (currentRetry == retryCount) // Here it is possible to check the type of exception if needed with an OR. And exit if it's a specific exception.
                            {
                                // We have tried as many times as retryCount specifies. Now we throw it and exit the application
                                logger.LogCritical($"Database migration failed after {retryCount} retries");
                                throw;
                            }

                        }
                        // Waiting 5 seconds before trying again
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        // If the kerberos container is restarted but the user database contains users from a previous build
        // Which isn't reflected by the internal kerberos database, then this method will
        // ensure that the kerberos database contains all the same users the user database contains
        private async Task ExistingPrincipalsDatabaseCheck(IApplicationBuilder app)
        {
            using(var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using(var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    // Npgsql resiliency strategy works here, so no retry pattern is needed
                    var users = await context.Users.ToListAsync();
                    foreach (var user in users)
                    {
                        Console.WriteLine("Recreating principal " + user.Username);
                        if (user.Username.Contains("/")) // User has been defined with a host, and is therefore a 'service'
                        {
                            var serviceName = user.Username.Split("/")[0];
                            var serviceHost = user.Username.Split("/")[1];
                            $"create-service.sh {serviceName} {serviceHost}".Bash();
                            var newUserKeyTabFilePath = $"/keytabs/{serviceName}.service.keytab";
                            user.KeyTabFile = File.ReadAllBytes(newUserKeyTabFilePath);
                        }
                        else
                        {
                            $"create-user.sh {user.Username}".Bash();
                            var newUserKeyTabFilePath = $"/keytabs/{user.Username}.user.keytab";
                            user.KeyTabFile = File.ReadAllBytes(newUserKeyTabFilePath);
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
