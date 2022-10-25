using Microsoft.EntityFrameworkCore;

namespace WebApi
{
    public class DbModel : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbModel()
        {
        }

        public DbModel(DbContextOptions options) : base(options)
        {
        }
    }
}
