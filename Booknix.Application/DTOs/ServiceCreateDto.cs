using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booknix.Application.DTOs
{
    public class ServiceCreateDto
    {
        [Required]
        public Guid LocationId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Required]
        public int ServiceGapMinutes { get; set; }  // Kullanıcıdan dakika olarak alınır

        // ✅ Yeni eklenen alan: çalışanlar
        public List<Guid> SelectedWorkerIds { get; set; } = new();
    }
}
