namespace GramKar.Models.ViewModels
{
    public class ReceiptItemViewModel
    {
        public string TaxName { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public decimal PrincipalPaid { get; set; }
        public decimal LateFeePaid { get; set; }
        public decimal ItemTotal => PrincipalPaid + LateFeePaid;
    }

    public class ReceiptViewModel
    {
        public int PaymentId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }

        public string PanchayatName { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
        public string Taluka { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string? Mobile { get; set; }

        public List<ReceiptItemViewModel> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public string AmountInWords { get; set; } = string.Empty;

        public string Mode { get; set; } = "Cash";
        public string? Reference { get; set; }

        public string ReceivedBy { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public string? CancelReason { get; set; }

        public decimal RemainingHouseholdBalance { get; set; }
    }
}
