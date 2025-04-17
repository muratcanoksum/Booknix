using System;
using System.Collections.Generic;

namespace Booknix.Domain.Entities
{
    public class Location
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public Guid SectorId { get; set; }

        // Navigation
        public Sector? Sector { get; set; }
        public ICollection<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<WorkingHour> WorkingHours { get; set; } = new List<WorkingHour>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();


    }
}
