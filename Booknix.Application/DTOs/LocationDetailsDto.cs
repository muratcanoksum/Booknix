using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class LocationDetailsDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
        public List<ServiceCardDto>? Services { get; set; }
    }

}
