using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booknix.Application.DTOs
{
    public class ServiceUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Required]
        public int ServiceGapMinutes { get; set; }  // Kullanýcýdan dakika olarak alýnýr

        public List<Guid> SelectedWorkerIds { get; set; } = new List<Guid>();
    }
}