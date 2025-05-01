using System;
using System.Collections.Generic;

namespace Booknix.Domain.Entities
{
    public class Sector
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public ICollection<Location> Locations { get; set; } = new List<Location>();

        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    }
}
