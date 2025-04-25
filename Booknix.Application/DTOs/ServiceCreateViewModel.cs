using Booknix.Domain.Entities;

namespace Booknix.Application.ViewModels;

public class ServiceCreateViewModel
{
    public Guid LocationId { get; set; }

    public List<Service> ServiceList { get; set; } = new();         // 📌 Eksik olan bu
    public List<Worker> AvailableWorkers { get; set; } = new();

    public List<Guid> SelectedWorkerIds { get; set; } = new();     // Optional: POST için
}
