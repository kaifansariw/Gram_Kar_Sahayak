using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.Entities;

namespace GramKar.Services
{
    public class AllocationPreviewItem
    {
        public int DemandId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public decimal DemandAmount { get; set; }
        public decimal PendingPrincipalBefore { get; set; }
        public decimal PendingLateFeeBefore { get; set; }
        public decimal PrincipalToPay { get; set; }
        public decimal LateFeeToPay { get; set; }
        public decimal TotalAllocated => PrincipalToPay + LateFeeToPay;
        public decimal RemainingPrincipalAfter => PendingPrincipalBefore - PrincipalToPay;
        public decimal RemainingLateFeeAfter => PendingLateFeeBefore - LateFeeToPay;
        public decimal RemainingPayableAfter => RemainingPrincipalAfter + RemainingLateFeeAfter;
    }

    public class PaymentAllocationPreview
    {
        public int HouseId { get; set; }
        public string HouseNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal TotalPayableBefore { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal RemainingBalanceAfter => TotalPayableBefore - TotalAllocated;
        public List<AllocationPreviewItem> Items { get; set; } = new();
    }

    public class PaymentService
    {
        private readonly AppDbContext _context;
        private readonly DuesCalculator _duesCalculator;

        public PaymentService(AppDbContext context, DuesCalculator duesCalculator)
        {
            _context = context;
            _duesCalculator = duesCalculator;
        }

        public async Task<PaymentAllocationPreview> GenerateAllocationPreviewAsync(
            int houseId, decimal paymentAmount, DateTime? paymentDate = null)
        {
            var date = paymentDate ?? DateTime.Today;

            var household = await _context.Households
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (household == null)
            {
                throw new ArgumentException("Household not found");
            }

            var summary = _duesCalculator.CalculateHouseholdSummary(household, date);
            var pendingDemands = summary.Demands
                .Where(d => d.TotalPayable > 0)
                .OrderBy(d => d.DueDate)
                .ThenBy(d => d.FinancialYear)
                .ToList();

            var preview = new PaymentAllocationPreview
            {
                HouseId = household.HouseId,
                HouseNo = household.HouseNo,
                OwnerName = household.OwnerName,
                TotalPayableBefore = summary.TotalPayable,
                PaymentAmount = paymentAmount
            };

            decimal remainingMoney = paymentAmount;

            foreach (var dem in pendingDemands)
            {
                if (remainingMoney <= 0) break;

                var item = new AllocationPreviewItem
                {
                    DemandId = dem.DemandId,
                    TaxName = dem.TaxName,
                    FinancialYear = dem.FinancialYear,
                    DemandAmount = dem.PrincipalAmount,
                    PendingPrincipalBefore = dem.PendingPrincipal,
                    PendingLateFeeBefore = dem.PendingLateFee
                };

                // 1. Late fee first
                if (dem.PendingLateFee > 0 && remainingMoney > 0)
                {
                    var feeToPay = Math.Min(remainingMoney, dem.PendingLateFee);
                    item.LateFeeToPay = Math.Round(feeToPay, 2);
                    remainingMoney -= item.LateFeeToPay;
                }

                // 2. Then principal
                if (dem.PendingPrincipal > 0 && remainingMoney > 0)
                {
                    var prinToPay = Math.Min(remainingMoney, dem.PendingPrincipal);
                    item.PrincipalToPay = Math.Round(prinToPay, 2);
                    remainingMoney -= item.PrincipalToPay;
                }

                if (item.TotalAllocated > 0)
                {
                    preview.Items.Add(item);
                }
            }

            preview.TotalAllocated = preview.Items.Sum(i => i.TotalAllocated);
            return preview;
        }

        public async Task<Payment> RecordPaymentAsync(
            int houseId,
            decimal amount,
            string mode,
            string? reference,
            int receivedByUserId,
            DateTime? paymentDate = null)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            var date = paymentDate ?? DateTime.Today;
            var preview = await GenerateAllocationPreviewAsync(houseId, amount, date);

            if (amount > preview.TotalPayableBefore + 0.01m)
            {
                throw new InvalidOperationException($"Payment amount (₹{amount:N2}) cannot exceed total pending dues (₹{preview.TotalPayableBefore:N2}).");
            }

            // Generate unique receipt number: GP-YYYY-XXXXXX
            var year = DateTime.Today.Year;
            var countThisYear = await _context.Payments.CountAsync(p => p.PaymentDate.Year == year) + 1;
            var receiptNo = $"GP-{year}-{countThisYear:D6}";

            // Ensure uniqueness
            while (await _context.Payments.AnyAsync(p => p.ReceiptNo == receiptNo))
            {
                countThisYear++;
                receiptNo = $"GP-{year}-{countThisYear:D6}";
            }

            var payment = new Payment
            {
                ReceiptNo = receiptNo,
                HouseId = houseId,
                PaymentDate = DateTime.Now,
                TotalAmount = amount,
                Mode = mode,
                Reference = reference,
                ReceivedByUserId = receivedByUserId,
                IsCancelled = false,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in preview.Items)
            {
                payment.PaymentAllocations.Add(new PaymentAllocation
                {
                    DemandId = item.DemandId,
                    PrincipalPaid = item.PrincipalToPay,
                    LateFeePaid = item.LateFeeToPay
                });
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> CancelPaymentAsync(int paymentId, string reason)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.IsCancelled)
            {
                return false;
            }

            payment.IsCancelled = true;
            payment.CancelReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled by user" : reason;
            payment.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
