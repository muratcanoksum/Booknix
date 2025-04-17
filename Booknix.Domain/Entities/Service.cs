using System;
using System.Collections.Generic;

namespace Booknix.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }

        public Guid LocationId { get; set; }

        public Location? Location { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ServiceEmployee> ServiceEmployees { get; set; } = new List<ServiceEmployee>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();


    }
}
