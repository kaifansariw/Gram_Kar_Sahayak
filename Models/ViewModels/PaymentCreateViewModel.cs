using System.ComponentModel.DataAnnotations;
using GramKar.Services;

namespace GramKar.Models.ViewModels
{
    public class PaymentCreateViewModel
    {
        public int? SelectedHouseId { get; set; }
        public string? SearchTerm { get; set; }

        public HouseholdDuesSummary? SelectedHouseholdSummary { get; set; }
        public PaymentAllocationPreview? AllocationPreview { get; set; }

        [Required(ErrorMessage = "Please enter payment amount")]
        [Range(1, 1000000, ErrorMessage = "Payment amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required]
        public string Mode { get; set; } = "Cash"; // Cash, UPI, Cheque

        [Display(Name = "Reference / Cheque Number")]
        public string? Reference { get; set; }
    }
}
