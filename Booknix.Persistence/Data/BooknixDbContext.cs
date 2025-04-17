using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Entities;

namespace Booknix.Persistence.Data
{
    public class BooknixDbContext : DbContext
    {
        public BooknixDbContext(DbContextOptions<BooknixDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>(); // 👈 eklendi

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - UserProfile birebir ilişki
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // User silinince profili de silinir

            // Sabit GUID'lerle rollerin seed edilmesi
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Admin" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Employee" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Client" }
            );
        }
    }
}
