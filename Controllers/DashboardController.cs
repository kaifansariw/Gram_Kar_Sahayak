using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.ViewModels;
using GramKar.Services;

namespace GramKar.Controllers
{
    [Authorize(Roles = "Admin,Sarpanch")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DuesCalculator _duesCalculator;

        public DashboardController(AppDbContext context, DuesCalculator duesCalculator)
        {
            _context = context;
            _duesCalculator = duesCalculator;
        }

        public async Task<IActionResult> Index()
        {
            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync();
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            // 1. Collected Today
            var paymentsToday = await _context.Payments
                .Where(p => !p.IsCancelled && p.PaymentDate >= todayStart && p.PaymentDate < todayEnd)
                .ToListAsync();
            var collectedToday = paymentsToday.Sum(p => p.TotalAmount);

            // 2. Collected This Year
            var currentYear = panchayat?.CurrentFinancialYear ?? "2025-26";
            var allocationsThisYear = await _context.PaymentAllocations
                .Where(pa => pa.Payment != null && !pa.Payment.IsCancelled && pa.Demand != null && pa.Demand.FinancialYear == currentYear)
                .ToListAsync();
            var collectedThisYear = allocationsThisYear.Sum(pa => pa.PrincipalPaid + pa.LateFeePaid);

            // 3. Households and Total Pending calculations
            var households = await _context.Households
                .Where(h => h.IsActive)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .ToListAsync();

            decimal totalPending = 0m;
            int defaulterCount = 0;

            foreach (var h in households)
            {
                var summary = _duesCalculator.CalculateHouseholdSummary(h);
                totalPending += summary.TotalPayable;
                if (summary.HasOverdueDues)
                {
                    defaulterCount++;
                }
            }

            // 4. Recent 6 Payments
            var recentPayments = await _context.Payments
                .Include(p => p.Household)
                .Include(p => p.ReceivedByUser)
                .OrderByDescending(p => p.PaymentDate)
                .Take(6)
                .ToListAsync();

            var fullName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "User";
            var role = User.IsInRole("Admin") ? "Admin" : "Sarpanch";

            var model = new DashboardViewModel
            {
                PanchayatName = panchayat?.PanchayatName ?? "ગ્રામ પંચાયત",
                VillageName = panchayat?.VillageName ?? "આણંદપર",
                CurrentFinancialYear = currentYear,
                UserFullName = fullName,
                UserRole = role,
                CollectedToday = collectedToday,
                CollectedThisYear = collectedThisYear,
                TotalPending = totalPending,
                DefaulterCount = defaulterCount,
                TotalHouseholdCount = households.Count,
                RecentPayments = recentPayments
            };

            return View(model);
        }
    }
}
