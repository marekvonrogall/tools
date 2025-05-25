using Microsoft.EntityFrameworkCore;
using vrmo.Models;
using Microsoft.AspNetCore.Identity;

namespace vrmo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            string? adminPassword = Environment.GetEnvironmentVariable("VRMO_ADMIN_PASSWORD");

            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                var adminUser = new User
                {
                    Id = 1,
                    Username = "vrmo-admin",
                    IsAdmin = true
                };

                var hasher = new PasswordHasher<User>();
                adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

                modelBuilder.Entity<User>().HasData(adminUser);
            }

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
