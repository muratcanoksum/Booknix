using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class SessionTerminateDto
    {
        public required string SessionKey { get; set; }
    }

}
