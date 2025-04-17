using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Persistence.Data
{
    public class BooknixDbContext : DbContext
    {
        public BooknixDbContext(DbContextOptions<BooknixDbContext> options)
            : base(options)
        {
        }

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

            // MediaFile ilişkileri
            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Location)
                .WithMany(l => l.MediaFiles)
                .HasForeignKey(m => m.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.User)
                .WithMany(u => u.MediaFiles)
                .HasForeignKey(m => m.UserId)
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

            // Seed veriler (Roller)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Admin" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Employee" },
                new Role { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Client" }
            );
        }
    }
}
