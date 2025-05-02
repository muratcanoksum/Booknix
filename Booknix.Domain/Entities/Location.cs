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
        public string PhoneNumber { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public TimeSpan LunchBreakStart { get; set; }
        public TimeSpan LunchBreakEnd { get; set; }

        // Navigation
        public Sector? Sector { get; set; }
        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<WorkingHour> WorkingHours { get; set; } = new List<WorkingHour>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();


    }
}
