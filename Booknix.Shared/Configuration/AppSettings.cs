using Microsoft.Extensions.Configuration;
using Booknix.Shared.Interfaces;

namespace Booknix.Shared.Configuration
{
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string BaseUrl => _configuration["App:BaseUrl"]!;
    }
}
