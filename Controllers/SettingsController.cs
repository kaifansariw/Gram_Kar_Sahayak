using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.Entities;
using GramKar.Models.ViewModels;
using GramKar.Services;

namespace GramKar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DemandGenerator _demandGenerator;

        public SettingsController(AppDbContext context, DemandGenerator demandGenerator)
        {
            _context = context;
            _demandGenerator = demandGenerator;
        }

        public async Task<IActionResult> TaxTypes()
        {
            var taxTypes = await _context.TaxTypes.ToListAsync();
            var totalHouses = await _context.Households.CountAsync(h => h.IsActive);
            var panchayat = await _context.PanchayatSettings.FirstOrDefaultAsync() ?? new PanchayatSetting();

            var model = new TaxSettingsViewModel
            {
                TaxTypes = taxTypes,
                TargetFinancialYear = "2026-27",
                TotalActiveHouseholds = totalHouses,
                PanchayatSetting = panchayat
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTaxType(int id)
        {
            var taxType = await _context.TaxTypes.FindAsync(id);
            if (taxType == null)
            {
                return NotFound();
            }
            return View(taxType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTaxType(TaxType model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taxType = await _context.TaxTypes.FindAsync(model.TaxTypeId);
            if (taxType == null)
            {
                return NotFound();
            }

            taxType.Name = model.Name.Trim();
            taxType.DefaultAmount = model.DefaultAmount;
            taxType.DueDay = model.DueDay;
            taxType.DueMonth = model.DueMonth;
            taxType.LateFeeType = model.LateFeeType;
            taxType.LateFeeValue = model.LateFeeValue;
            taxType.MaxLateFeePercent = model.MaxLateFeePercent;
            taxType.AppliesTo = model.AppliesTo;
            taxType.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"કર નિયમ '{taxType.Name}' સફળતાપૂર્વક અપડેટ થયો છે.";
            return RedirectToAction(nameof(TaxTypes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDues(string financialYear)
        {
            if (string.IsNullOrWhiteSpace(financialYear))
            {
                TempData["ErrorMessage"] = "નાણાકીય વર્ષ પસંદ કરવું જરૂરી છે (Financial year required).";
                return RedirectToAction(nameof(TaxTypes));
            }

            var result = await _demandGenerator.GenerateYearlyDemandsAsync(financialYear.Trim());

            // Also update current FY if needed
            var setting = await _context.PanchayatSettings.FirstOrDefaultAsync();
            if (setting != null)
            {
                setting.CurrentFinancialYear = financialYear.Trim();
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{financialYear} વર્ષ માટે {result.GeneratedCount} કરની માંગણીઓ જનરેટ થઈ! ({result.SkippedCount} પહેલેથી જ અસ્તિત્વમાં હતા)";
            return RedirectToAction(nameof(TaxTypes));
        }

        [HttpGet]
        public async Task<IActionResult> Panchayat()
        {
            var setting = await _context.PanchayatSettings.FirstOrDefaultAsync() ?? new PanchayatSetting();
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Panchayat(PanchayatSetting model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var setting = await _context.PanchayatSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                _context.PanchayatSettings.Add(model);
            }
            else
            {
                setting.PanchayatName = model.PanchayatName.Trim();
                setting.VillageName = model.VillageName.Trim();
                setting.Taluka = model.Taluka?.Trim() ?? "";
                setting.District = model.District?.Trim() ?? "";
                setting.SarpanchName = model.SarpanchName.Trim();
                setting.SarpanchMessage = model.SarpanchMessage.Trim();
                setting.OfficeAddress = model.OfficeAddress?.Trim() ?? "";
                setting.OfficeTimings = model.OfficeTimings?.Trim() ?? "";
                setting.OfficePhone = model.OfficePhone?.Trim() ?? "";
                setting.CurrentFinancialYear = model.CurrentFinancialYear?.Trim() ?? "2025-26";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "પંચાયત અને સરપંચની વિગતો સફળતાપૂર્વક સાચવવામાં આવી છે.";
            return RedirectToAction(nameof(Panchayat));
        }
    }
}
