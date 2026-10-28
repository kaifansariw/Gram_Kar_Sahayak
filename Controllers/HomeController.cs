using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.ViewModels;
using GramKar.Services;

namespace GramKar.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DuesCalculator _duesCalculator;
        private readonly PaymentService _paymentService;
        private readonly LocalizerService _loc;

        public HomeController(AppDbContext context, DuesCalculator duesCalculator, PaymentService paymentService, LocalizerService loc)
        {
            _context = context;
            _duesCalculator = duesCalculator;
            _paymentService = paymentService;
            _loc = loc;
        }

        public async Task<IActionResult> Index()
        {
            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync();
            ViewBag.Panchayat = panchayat;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckMyDues(string houseNo, string? mobile)
        {
            var model = new PublicDuesLookupViewModel
            {
                HouseNo = houseNo?.Trim() ?? string.Empty,
                Mobile = mobile?.Trim(),
                HasSearched = true
            };

            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync();
            if (panchayat != null)
            {
                model.OfficeAddress = panchayat.OfficeAddress;
                model.OfficeTimings = panchayat.OfficeTimings;
                model.OfficePhone = panchayat.OfficePhone;
            }

            if (string.IsNullOrWhiteSpace(model.HouseNo))
            {
                ModelState.AddModelError("HouseNo", "મહેરબાની કરીને ઘર નંબર દાખલ કરો (Please enter house number)");
                return PartialView("_DuesLookupResult", model);
            }

            var household = await _context.Households
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .FirstOrDefaultAsync(h => h.HouseNo.ToLower() == model.HouseNo.ToLower() && h.IsActive);

            if (household == null)
            {
                model.Found = false;
                return PartialView("_DuesLookupResult", model);
            }

            // Optional mobile check if provided
            if (!string.IsNullOrWhiteSpace(model.Mobile) && !string.IsNullOrWhiteSpace(household.Mobile))
            {
                var cleanEntered = new string(model.Mobile.Where(char.IsDigit).ToArray());
                var cleanStored = new string(household.Mobile.Where(char.IsDigit).ToArray());
                if (!cleanStored.EndsWith(cleanEntered) && !cleanEntered.EndsWith(cleanStored))
                {
                    model.Found = false;
                    return PartialView("_DuesLookupResult", model);
                }
            }

            var summary = _duesCalculator.CalculateHouseholdSummary(household);

            model.Found = true;
            model.HouseId = household.HouseId;
            model.PanchayatUpiId = "anandparpanchayat@sbi";
            model.PanchayatName = panchayat?.PanchayatName ?? "Gram Panchayat Anandpar";

            // Mask owner name to protect privacy (e.g. "જયં*** પટ**")
            var parts = household.OwnerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                model.MaskedOwnerName = parts[0] + (parts.Length > 1 ? " " + parts[^1][0] + "..." : "");
            }
            else
            {
                model.MaskedOwnerName = household.OwnerName;
            }

            model.Ward = household.Ward;
            model.TotalPending = summary.TotalPayable;
            model.PrincipalPending = summary.TotalPrincipalPending;
            model.LateFeePending = summary.TotalLateFeePending;
            model.PendingTaxNames = summary.Demands
                .Where(d => d.TotalPayable > 0)
                .Select(d => $"{_loc.LocalizeTaxName(d.TaxName)} ({d.FinancialYear}): ₹{d.TotalPayable:N0}")
                .ToList();

            return PartialView("_DuesLookupResult", model);
        }

        public class OnlinePaymentSubmitModel
        {
            public int HouseId { get; set; }
            public decimal Amount { get; set; }
            public string? PayerName { get; set; }
            public string? PayerMobile { get; set; }
            public string? UpiRef { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOnlinePayment([FromForm] OnlinePaymentSubmitModel model)
        {
            if (model.HouseId <= 0 || model.Amount <= 0)
            {
                return Json(new { success = false, message = "ચુકવણીની રકમ અમાન્ય છે (Invalid payment parameters)." });
            }

            var household = await _context.Households.FirstOrDefaultAsync(h => h.HouseId == model.HouseId && h.IsActive);
            if (household == null)
            {
                return Json(new { success = false, message = "ઘર મળ્યું નથી (Household not found)." });
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin")
                         ?? await _context.Users.FirstOrDefaultAsync();
            int receivedByUserId = adminUser?.UserId ?? 1;

            var upiRef = string.IsNullOrWhiteSpace(model.UpiRef)
                ? "UPI" + DateTime.Now.ToString("yyyyMMddHHmmss")
                : model.UpiRef.Trim();

            var payer = string.IsNullOrWhiteSpace(model.PayerName) ? household.OwnerName : model.PayerName.Trim();
            var mobile = string.IsNullOrWhiteSpace(model.PayerMobile) ? (household.Mobile ?? "Online") : model.PayerMobile.Trim();
            var reference = $"Online UPI ({upiRef}) - {payer} ({mobile})";

            try
            {
                var payment = await _paymentService.RecordPaymentAsync(
                    model.HouseId,
                    model.Amount,
                    "UPI",
                    reference,
                    receivedByUserId);

                return Json(new
                {
                    success = true,
                    receiptId = payment.PaymentId,
                    receiptNo = payment.ReceiptNo,
                    redirectUrl = Url.Action("Receipt", "Payments", new { id = payment.PaymentId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult SetLanguage(string lang, string returnUrl = "/")
        {
            if (lang != "gu" && lang != "hi" && lang != "en")
            {
                lang = "gu";
            }

            Response.Cookies.Append("GramKar_Lang", lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }
    }
}
