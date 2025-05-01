using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class SectorLocationsDto
    {
        public string? SectorName { get; set; }
        public List<LocationCardDto>? Locations { get; set; }
    }

}
