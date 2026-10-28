using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.ViewModels;
using GramKar.Services;

namespace GramKar.Controllers
{
    [Authorize(Roles = "Admin,Sarpanch")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DuesCalculator _duesCalculator;
        private readonly LocalizerService _localizer;

        public ReportsController(AppDbContext context, DuesCalculator duesCalculator, LocalizerService localizer)
        {
            _context = context;
            _duesCalculator = duesCalculator;
            _localizer = localizer;
        }

        public async Task<IActionResult> Pending(
            string? financialYear,
            string? ward,
            int? taxTypeId,
            decimal? minAmount,
            string sortBy = "amount_desc")
        {
            var model = await BuildPendingReportAsync(financialYear, ward, taxTypeId, minAmount, sortBy);
            return View(model);
        }

        public async Task<IActionResult> PrintPending(
            string? financialYear,
            string? ward,
            int? taxTypeId,
            decimal? minAmount,
            string sortBy = "amount_desc")
        {
            var model = await BuildPendingReportAsync(financialYear, ward, taxTypeId, minAmount, sortBy);
            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync();
            ViewBag.Panchayat = panchayat;
            return View(model);
        }

        public async Task<IActionResult> ExportCsv(
            string? financialYear,
            string? ward,
            int? taxTypeId,
            decimal? minAmount,
            string sortBy = "amount_desc")
        {
            var report = await BuildPendingReportAsync(financialYear, ward, taxTypeId, minAmount, sortBy);

            var records = report.Rows.Select(r => new
            {
                HouseNo = r.HouseNo,
                OwnerName = r.OwnerName,
                Ward = r.Ward,
                Mobile = r.Mobile ?? "-",
                TaxBreakdown = r.TaxBreakdown,
                PrincipalPending = r.PrincipalPending,
                LateFeePending = r.LateFeePending,
                TotalPending = r.TotalPending,
                OldestDueDate = r.OldestDueDate.HasValue ? r.OldestDueDate.Value.ToString("dd/MM/yyyy") : "-",
                DaysOverdue = r.DaysOverdue
            }).ToList();

            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
            }

            var filename = $"GramKar_Defaulters_{DateTime.Today:yyyyMMdd}.csv";
            return File(memoryStream.ToArray(), "text/csv", filename);
        }

        private async Task<PendingReportViewModel> BuildPendingReportAsync(
            string? financialYear,
            string? ward,
            int? taxTypeId,
            decimal? minAmount,
            string sortBy)
        {
            var householdsQuery = _context.Households
                .Where(h => h.IsActive)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ward) && ward != "All")
            {
                householdsQuery = householdsQuery.Where(h => h.Ward == ward);
            }

            var households = await householdsQuery.ToListAsync();

            var availableYears = await _context.Demands
                .Select(d => d.FinancialYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            var availableWards = await _context.Households
                .Select(h => h.Ward)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();

            var availableTaxTypes = await _context.TaxTypes
                .Where(t => t.IsActive)
                .ToListAsync();

            var rows = new List<PendingReportRow>();

            foreach (var h in households)
            {
                var summary = _duesCalculator.CalculateHouseholdSummary(h);

                var pendingDemands = summary.Demands.Where(d => d.TotalPayable > 0).AsEnumerable();

                if (!string.IsNullOrWhiteSpace(financialYear) && financialYear != "All")
                {
                    pendingDemands = pendingDemands.Where(d => d.FinancialYear == financialYear);
                }

                if (taxTypeId.HasValue && taxTypeId.Value > 0)
                {
                    var selectedTax = availableTaxTypes.FirstOrDefault(t => t.TaxTypeId == taxTypeId.Value);
                    if (selectedTax != null)
                    {
                        pendingDemands = pendingDemands.Where(d => d.TaxName == selectedTax.Name);
                    }
                }

                var demandList = pendingDemands.ToList();
                if (!demandList.Any()) continue;

                var totalPrincipal = demandList.Sum(d => d.PendingPrincipal);
                var totalLateFee = demandList.Sum(d => d.PendingLateFee);
                var totalPending = totalPrincipal + totalLateFee;

                if (minAmount.HasValue && totalPending < minAmount.Value)
                {
                    continue;
                }

                var overdueDemands = demandList.Where(d => d.IsOverdue).OrderBy(d => d.DueDate).ToList();
                var oldestDueDate = overdueDemands.Select(d => (DateTime?)d.DueDate).FirstOrDefault();
                int maxDaysOverdue = overdueDemands.Any() ? overdueDemands.Max(d => d.DaysOverdue) : 0;

                var breakdownStr = string.Join(" | ", demandList.Select(d => $"{_localizer.LocalizeTaxName(d.TaxName)} ({d.FinancialYear}): ₹{d.TotalPayable:N0}"));

                rows.Add(new PendingReportRow
                {
                    HouseId = h.HouseId,
                    HouseNo = h.HouseNo,
                    OwnerName = h.OwnerName,
                    Ward = h.Ward,
                    Mobile = h.Mobile,
                    TaxBreakdown = breakdownStr,
                    PrincipalPending = totalPrincipal,
                    LateFeePending = totalLateFee,
                    TotalPending = totalPending,
                    OldestDueDate = oldestDueDate,
                    DaysOverdue = maxDaysOverdue
                });
            }

            // Sorting
            rows = sortBy switch
            {
                "amount_asc" => rows.OrderBy(r => r.TotalPending).ToList(),
                "oldest_due" => rows.OrderByDescending(r => r.DaysOverdue).ThenByDescending(r => r.TotalPending).ToList(),
                _ => rows.OrderByDescending(r => r.TotalPending).ToList()
            };

            return new PendingReportViewModel
            {
                Filter = new PendingReportFilter
                {
                    FinancialYear = financialYear ?? "All",
                    Ward = ward ?? "All",
                    TaxTypeId = taxTypeId,
                    MinAmount = minAmount,
                    SortBy = sortBy
                },
                AvailableYears = availableYears,
                AvailableWards = availableWards,
                AvailableTaxTypes = availableTaxTypes,
                TotalDefaulterCount = rows.Count,
                TotalPendingAmount = rows.Sum(r => r.TotalPending),
                TotalLateFeeAmount = rows.Sum(r => r.LateFeePending),
                Rows = rows
            };
        }
    }
}
