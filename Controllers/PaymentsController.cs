using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.Entities;
using GramKar.Models.ViewModels;
using GramKar.Services;

namespace GramKar.Controllers
{
    [Authorize(Roles = "Admin,Sarpanch")]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaymentService _paymentService;
        private readonly DuesCalculator _duesCalculator;

        public PaymentsController(
            AppDbContext context,
            PaymentService paymentService,
            DuesCalculator duesCalculator)
        {
            _context = context;
            _paymentService = paymentService;
            _duesCalculator = duesCalculator;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Payments
                .Include(p => p.Household)
                .Include(p => p.ReceivedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.ReceiptNo.ToLower().Contains(term) ||
                                         (p.Household != null && (p.Household.HouseNo.ToLower().Contains(term) ||
                                                                  p.Household.OwnerName.ToLower().Contains(term))));
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.Search = search;
            return View(payments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create(int? houseId, string? search)
        {
            var model = new PaymentCreateViewModel
            {
                SelectedHouseId = houseId,
                SearchTerm = search
            };

            if (houseId.HasValue)
            {
                var household = await _context.Households
                    .Include(h => h.Demands)
                        .ThenInclude(d => d.TaxType)
                    .Include(h => h.Demands)
                        .ThenInclude(d => d.PaymentAllocations)
                            .ThenInclude(pa => pa.Payment)
                    .FirstOrDefaultAsync(h => h.HouseId == houseId.Value);

                if (household != null)
                {
                    model.SelectedHouseholdSummary = _duesCalculator.CalculateHouseholdSummary(household);
                    model.Amount = model.SelectedHouseholdSummary.TotalPayable;
                }
                else
                {
                    TempData["ErrorMessage"] = "ઘર મળ્યું નથી (Household not found).";
                }
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                var rawTerm = search.Trim();
                var term = rawTerm.ToLower();
                var cleanTerm = term.Replace("-", "").Replace(" ", "").Replace("_", "");

                var allHouses = await _context.Households.ToListAsync();
                var matches = allHouses
                    .Where(h =>
                        h.HouseNo.ToLower() == term ||
                        h.HouseNo.ToLower().Replace("-", "").Replace(" ", "").Replace("_", "") == cleanTerm ||
                        (h.Mobile != null && (h.Mobile == rawTerm || h.Mobile.Contains(rawTerm))) ||
                        h.OwnerName.ToLower().Contains(term) ||
                        (h.GuardianName != null && h.GuardianName.ToLower().Contains(term))
                    )
                    .OrderBy(h => h.HouseNo)
                    .ToList();

                if (matches.Count == 1)
                {
                    return RedirectToAction(nameof(Create), new { houseId = matches[0].HouseId, search = rawTerm });
                }
                else if (matches.Count > 1)
                {
                    ViewBag.MatchingHouseholds = matches;
                }
                else
                {
                    ViewBag.SearchNotFound = true;
                    TempData["ErrorMessage"] = $"'{rawTerm}' માટે કોઈ ઘર મળ્યું નથી (No household found matching '{rawTerm}').";
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllocationPreview(int houseId, decimal amount)
        {
            try
            {
                var preview = await _paymentService.GenerateAllocationPreviewAsync(houseId, amount);
                return Json(new { success = true, preview });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel model)
        {
            if (!model.SelectedHouseId.HasValue || model.SelectedHouseId.Value <= 0)
            {
                ModelState.AddModelError("", "મહેરબાની કરીને ઘર પસંદ કરો (Please select a household).");
                return View(model);
            }

            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "ચુકવણીની રકમ શૂન્ય કરતા વધુ હોવી જોઈએ (Payment amount must be greater than zero).");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);

            try
            {
                var payment = await _paymentService.RecordPaymentAsync(
                    model.SelectedHouseId.Value,
                    model.Amount,
                    model.Mode,
                    model.Reference,
                    userId > 0 ? userId : 1);

                TempData["SuccessMessage"] = $"પહોંચ {payment.ReceiptNo} સફળતાપૂર્વક બની ગઈ છે!";
                return RedirectToAction(nameof(Receipt), new { id = payment.PaymentId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                // Reload household summary
                var household = await _context.Households
                    .Include(h => h.Demands)
                        .ThenInclude(d => d.TaxType)
                    .Include(h => h.Demands)
                        .ThenInclude(d => d.PaymentAllocations)
                            .ThenInclude(pa => pa.Payment)
                    .FirstOrDefaultAsync(h => h.HouseId == model.SelectedHouseId.Value);

                if (household != null)
                {
                    model.SelectedHouseholdSummary = _duesCalculator.CalculateHouseholdSummary(household);
                }

                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Receipt(int? id)
        {
            var query = _context.Payments
                .Include(p => p.Household)
                .Include(p => p.ReceivedByUser)
                .Include(p => p.PaymentAllocations)
                    .ThenInclude(pa => pa.Demand)
                        .ThenInclude(d => d!.TaxType);

            Payment? payment = null;
            if (id.HasValue && id.Value > 0)
            {
                payment = await query.FirstOrDefaultAsync(p => p.PaymentId == id.Value);
            }
            else
            {
                payment = await query.OrderByDescending(p => p.PaymentDate).FirstOrDefaultAsync();
            }

            if (payment == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync();

            // Calculate remaining balance for this household
            var household = await _context.Households
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .FirstOrDefaultAsync(h => h.HouseId == payment.HouseId);

            decimal remainingBalance = 0m;
            if (household != null)
            {
                var summary = _duesCalculator.CalculateHouseholdSummary(household);
                remainingBalance = summary.TotalPayable;
            }

            var model = new ReceiptViewModel
            {
                PaymentId = payment.PaymentId,
                ReceiptNo = payment.ReceiptNo,
                PaymentDate = payment.PaymentDate,
                PanchayatName = panchayat?.PanchayatName ?? "ગ્રામ પંચાયત આણંદપર",
                VillageName = panchayat?.VillageName ?? "આણંદપર",
                Taluka = panchayat?.Taluka ?? "રાજકોટ",
                District = panchayat?.District ?? "રાજકોટ",
                HouseNo = payment.Household?.HouseNo ?? "",
                OwnerName = payment.Household?.OwnerName ?? "",
                GuardianName = payment.Household?.GuardianName ?? "",
                Ward = payment.Household?.Ward ?? "",
                Mobile = payment.Household?.Mobile,
                TotalAmount = payment.TotalAmount,
                AmountInWords = ConvertAmountToWords((long)payment.TotalAmount),
                Mode = payment.Mode,
                Reference = payment.Reference,
                ReceivedBy = payment.ReceivedByUser?.FullName ?? "તલાટી કમ મંત્રી",
                IsCancelled = payment.IsCancelled,
                CancelReason = payment.CancelReason,
                RemainingHouseholdBalance = remainingBalance,
                Items = payment.PaymentAllocations.Select(pa => new ReceiptItemViewModel
                {
                    TaxName = pa.Demand?.TaxType?.Name ?? "કર",
                    FinancialYear = pa.Demand?.FinancialYear ?? "",
                    PrincipalPaid = pa.PrincipalPaid,
                    LateFeePaid = pa.LateFeePaid
                }).ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "રદ કરવાનું કારણ જણાવવું જરૂરી છે (Cancellation reason is required).";
                return RedirectToAction(nameof(Receipt), new { id });
            }

            var success = await _paymentService.CancelPaymentAsync(id, reason);
            if (success)
            {
                TempData["SuccessMessage"] = "પહોંચ સફળતાપૂર્વક રદ કરવામાં આવી છે (Payment cancelled successfully).";
            }
            else
            {
                TempData["ErrorMessage"] = "પહોંચ રદ કરવામાં નિષ્ફળતા (Failed to cancel payment).";
            }

            return RedirectToAction(nameof(Receipt), new { id });
        }

        private static string ConvertAmountToWords(long number)
        {
            if (number == 0) return "Zero Rupees Only";
            if (number < 0) return "Minus " + ConvertAmountToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += ConvertAmountToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += ConvertAmountToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += ConvertAmountToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ConvertAmountToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim() + " Rupees Only";
        }
    }
}
