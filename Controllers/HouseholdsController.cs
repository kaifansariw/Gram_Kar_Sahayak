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
    public class HouseholdsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DuesCalculator _duesCalculator;

        public HouseholdsController(AppDbContext context, DuesCalculator duesCalculator)
        {
            _context = context;
            _duesCalculator = duesCalculator;
        }

        public async Task<IActionResult> Index(string? search, string? ward)
        {
            var query = _context.Households
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(h => h.HouseNo.ToLower().Contains(term) ||
                                         h.OwnerName.ToLower().Contains(term) ||
                                         (h.Mobile != null && h.Mobile.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(ward) && ward != "All")
            {
                query = query.Where(h => h.Ward == ward);
            }

            var households = await query.OrderBy(h => h.HouseNo).ToListAsync();
            var availableWards = await _context.Households
                .Select(h => h.Ward)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();

            var items = households.Select(h =>
            {
                var summary = _duesCalculator.CalculateHouseholdSummary(h);
                return new HouseholdItemViewModel
                {
                    HouseId = h.HouseId,
                    HouseNo = h.HouseNo,
                    OwnerName = h.OwnerName,
                    GuardianName = h.GuardianName,
                    Mobile = h.Mobile,
                    Ward = h.Ward,
                    PropertyType = h.PropertyType,
                    HasWater = h.HasWater,
                    IsActive = h.IsActive,
                    PendingAmount = summary.TotalPayable,
                    HasOverdue = summary.HasOverdueDues
                };
            }).ToList();

            var model = new HouseholdListViewModel
            {
                SearchQuery = search,
                WardFilter = ward,
                AvailableWards = availableWards,
                Households = items
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new HouseholdEditViewModel { IsActive = true, HasWater = true, PropertyType = "Residential" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HouseholdEditViewModel model)
        {
            if (await _context.Households.AnyAsync(h => h.HouseNo.ToLower() == model.HouseNo.Trim().ToLower()))
            {
                ModelState.AddModelError("HouseNo", "આ ઘર નંબર પહેલેથી જ અસ્તિત્વમાં છે (House number already exists).");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var household = new Household
            {
                HouseNo = model.HouseNo.Trim(),
                OwnerName = model.OwnerName.Trim(),
                GuardianName = model.GuardianName?.Trim() ?? string.Empty,
                Mobile = model.Mobile?.Trim(),
                Ward = model.Ward.Trim(),
                PropertyType = model.PropertyType,
                HasWater = model.HasWater,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Households.Add(household);
            await _context.SaveChangesAsync();

            // Auto-generate dues for active tax types for current year
            var setting = await _context.PanchayatSettings.FirstOrDefaultAsync();
            var currentYear = setting?.CurrentFinancialYear ?? "2025-26";
            var generator = new DemandGenerator(_context);
            await generator.GenerateYearlyDemandsAsync(currentYear);

            TempData["SuccessMessage"] = $"ઘર નંબર {household.HouseNo} સફળતાપૂર્વક ઉમેરવામાં આવ્યું છે.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var household = await _context.Households.FindAsync(id);
            if (household == null)
            {
                return NotFound();
            }

            var model = new HouseholdEditViewModel
            {
                HouseId = household.HouseId,
                HouseNo = household.HouseNo,
                OwnerName = household.OwnerName,
                GuardianName = household.GuardianName,
                Mobile = household.Mobile,
                Ward = household.Ward,
                PropertyType = household.PropertyType,
                HasWater = household.HasWater,
                IsActive = household.IsActive
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HouseholdEditViewModel model)
        {
            if (await _context.Households.AnyAsync(h => h.HouseId != model.HouseId && h.HouseNo.ToLower() == model.HouseNo.Trim().ToLower()))
            {
                ModelState.AddModelError("HouseNo", "આ ઘર નંબર અન્ય મિલકત સાથે નોંધાયેલ છે (House number already in use).");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var household = await _context.Households.FindAsync(model.HouseId);
            if (household == null)
            {
                return NotFound();
            }

            household.HouseNo = model.HouseNo.Trim();
            household.OwnerName = model.OwnerName.Trim();
            household.GuardianName = model.GuardianName?.Trim() ?? string.Empty;
            household.Mobile = model.Mobile?.Trim();
            household.Ward = model.Ward.Trim();
            household.PropertyType = model.PropertyType;
            household.HasWater = model.HasWater;
            household.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"ઘર નંબર {household.HouseNo} ની વિગતો અપડેટ થઈ ગઈ છે.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var household = await _context.Households.FindAsync(id);
            if (household != null)
            {
                household.IsActive = !household.IsActive;
                await _context.SaveChangesAsync();
                var status = household.IsActive ? "સક્રિય (Active)" : "નિષ્ક્રિય (Deactivated)";
                TempData["SuccessMessage"] = $"ઘર નંબર {household.HouseNo} હવે {status} છે.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var household = await _context.Households
                .Include(h => h.Demands)
                    .ThenInclude(d => d.TaxType)
                .Include(h => h.Demands)
                    .ThenInclude(d => d.PaymentAllocations)
                        .ThenInclude(pa => pa.Payment)
                .Include(h => h.Payments)
                    .ThenInclude(p => p.ReceivedByUser)
                .FirstOrDefaultAsync(h => h.HouseId == id);

            if (household == null)
            {
                return NotFound();
            }

            var summary = _duesCalculator.CalculateHouseholdSummary(household);

            var model = new HouseholdDetailsViewModel
            {
                Household = new HouseholdEditViewModel
                {
                    HouseId = household.HouseId,
                    HouseNo = household.HouseNo,
                    OwnerName = household.OwnerName,
                    GuardianName = household.GuardianName,
                    Mobile = household.Mobile,
                    Ward = household.Ward,
                    PropertyType = household.PropertyType,
                    HasWater = household.HasWater,
                    IsActive = household.IsActive
                },
                DuesSummary = summary,
                PaymentHistory = household.Payments.OrderByDescending(p => p.PaymentDate).ToList()
            };

            return View(model);
        }
    }
}
