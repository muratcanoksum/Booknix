using Booknix.Domain.Entities;


namespace Booknix.Application.DTOs
{
    public class SecurityViewModel
    {
        public List<UserSession> Sessions { get; set; } = [];
        public PagedAuditLogResult InitialAuditLogs { get; set; } = new();
    }


}
