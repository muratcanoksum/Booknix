using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Persistence.Data
{
    public class BooknixDbContext(DbContextOptions<BooknixDbContext> options) : DbContext(options)
    {

        // DbSet'ler
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>(); // 👈 eklendi
        public DbSet<Sector> Sectors => Set<Sector>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<UserLocation> UserLocations => Set<UserLocation>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceEmployee> ServiceEmployees => Set<ServiceEmployee>();
        public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();
        public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
        
        public DbSet<TrustedIp> TrustedIps => Set<TrustedIp>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - UserProfile birebir ilişki
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // User silinince profili de silinir

            // User - MediaFile ilişkisi
            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.User)
                .WithMany(u => u.MediaFiles)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - UserLocation ilişkisi
            modelBuilder.Entity<UserLocation>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLocations)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Notification ilişkisi
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Review ilişkisi
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Appointment ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - ServiceEmployee ilişkisi (Employee = User)
            modelBuilder.Entity<ServiceEmployee>()
                .HasOne(se => se.Employee)
                .WithMany(u => u.ServiceEmployees)
                .HasForeignKey(se => se.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ENUM: LocationRole
            modelBuilder.Entity<UserLocation>()
                .Property(u => u.RoleInLocation)
                .HasConversion<int>();

            // ENUM: AppointmentStatus
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<int>();

            // ENUM: SlotStatus
            modelBuilder.Entity<AppointmentSlot>()
                .Property(s => s.Status)
                .HasConversion<int>();

            // MediaFile - Location/Service/Sector ilişkileri
            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Location)
                .WithMany(l => l.MediaFiles)
                .HasForeignKey(m => m.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Service)
                .WithMany(s => s.MediaFiles)
                .HasForeignKey(m => m.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Sector)
                .WithMany(s => s.MediaFiles)
                .HasForeignKey(m => m.SectorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.AdminUser)
                .WithMany()
                .HasForeignKey(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.SetNull);




            // Roller (Seed)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Admin" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Employee" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Client" }
            );
        }

    }
}
