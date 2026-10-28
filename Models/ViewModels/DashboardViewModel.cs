using GramKar.Models.Entities;

namespace GramKar.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string PanchayatName { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
        public string CurrentFinancialYear { get; set; } = "2025-26";
        public string UserFullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Admin";

        public decimal CollectedToday { get; set; }
        public decimal CollectedThisYear { get; set; }
        public decimal TotalPending { get; set; }
        public int DefaulterCount { get; set; }
        public int TotalHouseholdCount { get; set; }

        public decimal TotalDemanded => CollectedThisYear + TotalPending;
        public int CollectionPercentage => TotalDemanded > 0 
            ? (int)Math.Min(100, Math.Round((CollectedThisYear / TotalDemanded) * 100)) 
            : 0;

        public List<Payment> RecentPayments { get; set; } = new();
    }
}
