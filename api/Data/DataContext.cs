using Microsoft.EntityFrameworkCore;
using app_api.Data.Models;

namespace app_api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}
        public DbSet<User> Users { get; set; }
    }
}