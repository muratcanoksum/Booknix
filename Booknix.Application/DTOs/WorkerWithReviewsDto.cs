using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.DTOs
{
    public class WorkerWithReviewsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public LocationRole RoleInLocation { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }

        // Static method to convert Worker to DTO
        public static WorkerWithReviewsDto FromWorker(Worker worker)
        {
            return new WorkerWithReviewsDto
            {
                Id = worker.Id,
                FullName = worker.User?.FullName ?? string.Empty,
                Email = worker.User?.Email ?? string.Empty,
                RoleInLocation = worker.RoleInLocation,
                ReviewCount = 0,
                AverageRating = 0
            };
        }
    }
} 