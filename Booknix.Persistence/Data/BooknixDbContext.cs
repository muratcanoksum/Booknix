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
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Sector> Sectors => Set<Sector>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Worker> Workers => Set<Worker>();
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

            // User - UserProfile (1-1)
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Worker (1-1)
            modelBuilder.Entity<Worker>()
                .HasOne(w => w.User)
                .WithOne() // çünkü User navigation yok
                .HasForeignKey<Worker>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // Worker - Location (N-1)
            modelBuilder.Entity<Worker>()
                .HasOne(w => w.Location)
                .WithMany(l => l.Workers)
                .HasForeignKey(w => w.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ENUM: Worker.LocationRole
            modelBuilder.Entity<Worker>()
                .Property(w => w.RoleInLocation)
                .HasConversion<int>();

            // User - Notification (1-N)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Review (1-N)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Appointment (1-N)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceEmployee: Service ↔ Worker
            modelBuilder.Entity<ServiceEmployee>()
                .HasOne(se => se.Service)
                .WithMany(s => s.ServiceEmployees)
                .HasForeignKey(se => se.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceEmployee>()
                .HasOne(se => se.Worker)
                .WithMany() // Worker'da koleksiyon yoksa bu yeterli
                .HasForeignKey(se => se.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceEmployee>()
                .HasIndex(se => new { se.ServiceId, se.WorkerId })
                .IsUnique();

            // ENUM: Appointment.Status
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<int>();

            // ENUM: AppointmentSlot.Status
            modelBuilder.Entity<AppointmentSlot>()
                .Property(s => s.Status)
                .HasConversion<int>();

            // MediaFile ilişkileri
            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.User)
                .WithMany(u => u.MediaFiles)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Seed roller
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Admin" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Employee" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Client" }
            );
        }
    }
}
