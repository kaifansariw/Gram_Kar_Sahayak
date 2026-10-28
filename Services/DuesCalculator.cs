using GramKar.Models.Entities;

namespace GramKar.Services
{
    public class CalculatedDemandStatus
    {
        public int DemandId { get; set; }
        public int HouseId { get; set; }
        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public int MonthsOverdue { get; set; }

        public decimal PaidPrincipal { get; set; }
        public decimal PaidLateFee { get; set; }

        public decimal CalculatedTotalLateFee { get; set; }
        public decimal PendingLateFee { get; set; }
        public decimal PendingPrincipal { get; set; }
        public decimal TotalPayable { get; set; }

        public bool IsFullyPaid => TotalPayable <= 0.005m;
        public bool IsPartial => PaidPrincipal > 0 || PaidLateFee > 0;
    }

    public class HouseholdDuesSummary
    {
        public int HouseId { get; set; }
        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal TotalPrincipalPending { get; set; }
        public decimal TotalLateFeePending { get; set; }
        public decimal TotalPayable { get; set; }
        public bool HasOverdueDues { get; set; }
        public DateTime? OldestDueDate { get; set; }
        public List<CalculatedDemandStatus> Demands { get; set; } = new();
    }

    public class DuesCalculator
    {
        /// <summary>
        /// Calculates dues for a single demand based on payments and today's date.
        /// </summary>
        public CalculatedDemandStatus CalculateDemandDues(Demand demand, DateTime? asOfDate = null)
        {
            var today = (asOfDate ?? DateTime.Today).Date;
            var dueDate = demand.DueDate.Date;

            // Sum up active (non-cancelled) payment allocations
            var activeAllocations = demand.PaymentAllocations
                .Where(pa => pa.Payment != null && !pa.Payment.IsCancelled)
                .ToList();

            var paidPrincipal = activeAllocations.Sum(pa => pa.PrincipalPaid);
            var paidLateFee = activeAllocations.Sum(pa => pa.LateFeePaid);

            var pendingPrincipal = Math.Max(0m, demand.Amount - paidPrincipal);

            decimal calculatedLateFee = 0m;
            int daysOverdue = 0;
            int monthsLate = 0;
            bool isOverdue = false;

            if (today > dueDate && pendingPrincipal > 0)
            {
                isOverdue = true;
                daysOverdue = (today - dueDate).Days;
                // ceil(days overdue / 30)
                monthsLate = (int)Math.Ceiling(daysOverdue / 30.0);
                if (monthsLate < 1) monthsLate = 1;

                var taxType = demand.TaxType;
                if (taxType != null)
                {
                    if (string.Equals(taxType.LateFeeType, "Flat", StringComparison.OrdinalIgnoreCase))
                    {
                        calculatedLateFee = taxType.LateFeeValue;
                    }
                    else // Percent per month
                    {
                        calculatedLateFee = pendingPrincipal * (taxType.LateFeeValue / 100m) * monthsLate;
                    }

                    // Optional cap: e.g. MaxLateFeePercent (default 50% of demand amount)
                    if (taxType.MaxLateFeePercent > 0)
                    {
                        var maxCap = demand.Amount * (taxType.MaxLateFeePercent / 100m);
                        if (calculatedLateFee > maxCap)
                        {
                            calculatedLateFee = maxCap;
                        }
                    }
                }
            }

            calculatedLateFee = Math.Round(calculatedLateFee, 2);
            var pendingLateFee = Math.Max(0m, calculatedLateFee - paidLateFee);
            var totalPayable = pendingPrincipal + pendingLateFee;

            return new CalculatedDemandStatus
            {
                DemandId = demand.DemandId,
                HouseId = demand.HouseId,
                HouseNo = demand.Household?.HouseNo ?? string.Empty,
                OwnerName = demand.Household?.OwnerName ?? string.Empty,
                Ward = demand.Household?.Ward ?? string.Empty,
                Mobile = demand.Household?.Mobile,
                TaxName = demand.TaxType?.Name ?? "Tax",
                FinancialYear = demand.FinancialYear,
                PrincipalAmount = demand.Amount,
                DueDate = dueDate,
                IsOverdue = isOverdue,
                DaysOverdue = daysOverdue,
                MonthsOverdue = monthsLate,
                PaidPrincipal = paidPrincipal,
                PaidLateFee = paidLateFee,
                CalculatedTotalLateFee = calculatedLateFee,
                PendingLateFee = pendingLateFee,
                PendingPrincipal = pendingPrincipal,
                TotalPayable = totalPayable
            };
        }

        /// <summary>
        /// Calculates all dues for a given household.
        /// </summary>
        public HouseholdDuesSummary CalculateHouseholdSummary(Household household, DateTime? asOfDate = null)
        {
            var calculatedDemands = household.Demands
                .Select(d => CalculateDemandDues(d, asOfDate))
                .OrderBy(d => d.DueDate)
                .ThenBy(d => d.FinancialYear)
                .ToList();

            var pendingDemands = calculatedDemands.Where(d => d.TotalPayable > 0).ToList();

            return new HouseholdDuesSummary
            {
                HouseId = household.HouseId,
                HouseNo = household.HouseNo,
                OwnerName = household.OwnerName,
                Ward = household.Ward,
                Mobile = household.Mobile,
                IsActive = household.IsActive,
                TotalPrincipalPending = pendingDemands.Sum(d => d.PendingPrincipal),
                TotalLateFeePending = pendingDemands.Sum(d => d.PendingLateFee),
                TotalPayable = pendingDemands.Sum(d => d.TotalPayable),
                HasOverdueDues = pendingDemands.Any(d => d.IsOverdue),
                OldestDueDate = pendingDemands.Where(d => d.IsOverdue).OrderBy(d => d.DueDate).Select(d => (DateTime?)d.DueDate).FirstOrDefault(),
                Demands = calculatedDemands
            };
        }
    }
}
