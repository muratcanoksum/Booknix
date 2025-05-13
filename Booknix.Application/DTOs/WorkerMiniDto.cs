using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class WorkerMiniDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }

}
