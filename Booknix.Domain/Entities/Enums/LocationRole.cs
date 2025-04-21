using System.ComponentModel.DataAnnotations;

namespace Booknix.Domain.Entities.Enums
{
    public enum LocationRole
    {
        [Display(Name = "Yönetici")]
        LocationAdmin = 0,

        [Display(Name = "Çalışan")]
        LocationEmployee = 1
    }
}
