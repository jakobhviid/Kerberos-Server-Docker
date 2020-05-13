using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using app_api.Data;
using app_api.V1.Repos.UserRepo;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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
            services.AddControllers();

            // adding Database
            var connectionString = Configuration["KERBEROS_MSSQL_CONNECTION_STRING"];
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
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
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // TODO
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            UpdateDatabase(app);
            if (env.IsDevelopment())
            {
                Console.WriteLine("Running in development mode");
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            await ExistingPrincipalsDatabaseCheck(app);

        }
        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

        // If the kerberos container is restarted but the user database contains users from a previous build
        // Which isn't reflected by the internal kerberos database, then this method will
        // ensure that the kerberos database contains all the same users the user database contains
        private async static Task ExistingPrincipalsDatabaseCheck(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
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
