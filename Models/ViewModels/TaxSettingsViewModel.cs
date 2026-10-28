using System.ComponentModel.DataAnnotations;
using GramKar.Models.Entities;

namespace GramKar.Models.ViewModels
{
    public class TaxSettingsViewModel
    {
        public List<TaxType> TaxTypes { get; set; } = new();
        public string TargetFinancialYear { get; set; } = "2026-27";
        public int TotalActiveHouseholds { get; set; }
        public PanchayatSetting PanchayatSetting { get; set; } = new();
    }

    public class PublicDuesLookupViewModel
    {
        [Required(ErrorMessage = "Please enter your house number")]
        [Display(Name = "House No.")]
        public string HouseNo { get; set; } = string.Empty;

        [Display(Name = "Registered Mobile Number (Optional verification)")]
        public string? Mobile { get; set; }

        public bool HasSearched { get; set; }
        public bool Found { get; set; }
        public int HouseId { get; set; }
        public string? MaskedOwnerName { get; set; }
        public string? Ward { get; set; }
        public decimal TotalPending { get; set; }
        public decimal PrincipalPending { get; set; }
        public decimal LateFeePending { get; set; }
        public List<string> PendingTaxNames { get; set; } = new();
        public string OfficeAddress { get; set; } = string.Empty;
        public string OfficeTimings { get; set; } = string.Empty;
        public string OfficePhone { get; set; } = string.Empty;
        public string PanchayatUpiId { get; set; } = "anandparpanchayat@sbi";
        public string PanchayatName { get; set; } = "Gram Panchayat Anandpar";
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
