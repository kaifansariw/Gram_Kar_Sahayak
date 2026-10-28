using GramKar.Services;

namespace GramKar.Models.ViewModels
{
    public class PendingReportFilter
    {
        public string? FinancialYear { get; set; }
        public string? Ward { get; set; }
        public int? TaxTypeId { get; set; }
        public decimal? MinAmount { get; set; }
        public string SortBy { get; set; } = "amount_desc"; // "amount_desc", "amount_asc", "oldest_due"
    }

    public class PendingReportRow
    {
        public int HouseId { get; set; }
        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string TaxBreakdown { get; set; } = string.Empty;
        public decimal PrincipalPending { get; set; }
        public decimal LateFeePending { get; set; }
        public decimal TotalPending { get; set; }
        public DateTime? OldestDueDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class PendingReportViewModel
    {
        public PendingReportFilter Filter { get; set; } = new();
        public List<string> AvailableYears { get; set; } = new();
        public List<string> AvailableWards { get; set; } = new();
        public List<Models.Entities.TaxType> AvailableTaxTypes { get; set; } = new();

        public int TotalDefaulterCount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalLateFeeAmount { get; set; }

        public List<PendingReportRow> Rows { get; set; } = new();
    }
}
