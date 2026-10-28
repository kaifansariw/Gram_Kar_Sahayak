using Microsoft.EntityFrameworkCore;
using GramKar.Data;
using GramKar.Models.Entities;

namespace GramKar.Services
{
    public class GenerationResult
    {
        public string FinancialYear { get; set; } = string.Empty;
        public int GeneratedCount { get; set; }
        public int SkippedCount { get; set; }
        public int TotalHouses { get; set; }
        public List<string> Messages { get; set; } = new();
    }

    public class DemandGenerator
    {
        private readonly AppDbContext _context;

        public DemandGenerator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GenerationResult> GenerateYearlyDemandsAsync(string financialYear)
        {
            var result = new GenerationResult
            {
                FinancialYear = financialYear
            };

            var households = await _context.Households
                .Where(h => h.IsActive)
                .ToListAsync();

            var taxTypes = await _context.TaxTypes
                .Where(t => t.IsActive)
                .ToListAsync();

            result.TotalHouses = households.Count;

            // Determine years for DueDate calculation
            // e.g. "2025-26" -> startYear = 2025, endYear = 2026
            int startYear = DateTime.Today.Year;
            int endYear = startYear + 1;

            if (financialYear.Contains('-'))
            {
                var parts = financialYear.Split('-');
                if (int.TryParse(parts[0], out int sy))
                {
                    startYear = sy;
                    if (parts.Length > 1 && parts[1].Length == 2 && int.TryParse(parts[1], out int eyShort))
                    {
                        endYear = (sy / 100) * 100 + eyShort;
                    }
                    else if (int.TryParse(parts[1], out int eyFull))
                    {
                        endYear = eyFull;
                    }
                }
            }
            else if (int.TryParse(financialYear, out int singleYear))
            {
                startYear = singleYear;
                endYear = singleYear + 1;
            }

            // Existing demands for this financial year to prevent duplicates
            var existingKeys = await _context.Demands
                .Where(d => d.FinancialYear == financialYear)
                .Select(d => $"{d.HouseId}_{d.TaxTypeId}")
                .ToListAsync();

            var existingSet = new HashSet<string>(existingKeys);
            var newDemands = new List<Demand>();

            foreach (var house in households)
            {
                foreach (var tax in taxTypes)
                {
                    // Check applicability
                    bool isApplicable = true;
                    if (string.Equals(tax.AppliesTo, "Water", StringComparison.OrdinalIgnoreCase))
                    {
                        isApplicable = house.HasWater;
                    }
                    else if (string.Equals(tax.AppliesTo, "Shop", StringComparison.OrdinalIgnoreCase))
                    {
                        isApplicable = string.Equals(house.PropertyType, "Shop", StringComparison.OrdinalIgnoreCase);
                    }

                    if (!isApplicable) continue;

                    var key = $"{house.HouseId}_{tax.TaxTypeId}";
                    if (existingSet.Contains(key))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // Calculate Due Date based on DueMonth and DueDay
                    int dueYear = (tax.DueMonth >= 4) ? startYear : endYear;
                    int safeDay = Math.Min(tax.DueDay, DateTime.DaysInMonth(dueYear, tax.DueMonth));
                    var dueDate = new DateTime(dueYear, tax.DueMonth, safeDay);

                    newDemands.Add(new Demand
                    {
                        HouseId = house.HouseId,
                        TaxTypeId = tax.TaxTypeId,
                        FinancialYear = financialYear,
                        Amount = tax.DefaultAmount,
                        DueDate = dueDate,
                        Remarks = "Auto-generated yearly dues",
                        CreatedAt = DateTime.UtcNow
                    });

                    existingSet.Add(key);
                    result.GeneratedCount++;
                }
            }

            if (newDemands.Any())
            {
                _context.Demands.AddRange(newDemands);
                await _context.SaveChangesAsync();
            }

            result.Messages.Add($"Successfully generated {result.GeneratedCount} dues across {result.TotalHouses} households. {result.SkippedCount} existing demands were kept unchanged.");
            return result;
        }
    }
}
