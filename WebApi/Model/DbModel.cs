using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ModuleType>()
                .HasDiscriminator<string>("class")
                .HasValue<PrimitiveType>("Primitive")
                .HasValue<EntityType>("Entity");

            base.OnModelCreating(builder);
        }
    }
}
