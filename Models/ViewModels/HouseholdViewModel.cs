using System.ComponentModel.DataAnnotations;
using GramKar.Services;

namespace GramKar.Models.ViewModels
{
    public class HouseholdListViewModel
    {
        public string? SearchQuery { get; set; }
        public string? WardFilter { get; set; }
        public List<string> AvailableWards { get; set; } = new();
        public List<HouseholdItemViewModel> Households { get; set; } = new();
    }

    public class HouseholdItemViewModel
    {
        public int HouseId { get; set; }
        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string Ward { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public bool HasWater { get; set; }
        public bool IsActive { get; set; }
        public decimal PendingAmount { get; set; }
        public bool HasOverdue { get; set; }
    }

    public class HouseholdEditViewModel
    {
        public int HouseId { get; set; }

        [Required(ErrorMessage = "House No. is required")]
        [Display(Name = "House No.")]
        public string HouseNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Owner Name is required")]
        [Display(Name = "Owner Name")]
        public string OwnerName { get; set; } = string.Empty;

        [Display(Name = "Father/Husband Name")]
        public string? GuardianName { get; set; }

        [Phone]
        [Display(Name = "Mobile Number")]
        public string? Mobile { get; set; }

        [Required(ErrorMessage = "Ward / Area is required")]
        [Display(Name = "Ward / Faliya")]
        public string Ward { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Type")]
        public string PropertyType { get; set; } = "Residential";

        [Display(Name = "Has Piped Water Connection")]
        public bool HasWater { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public class HouseholdDetailsViewModel
    {
        public HouseholdEditViewModel Household { get; set; } = new();
        public HouseholdDuesSummary DuesSummary { get; set; } = new();
        public List<Models.Entities.Payment> PaymentHistory { get; set; } = new();
    }
}
