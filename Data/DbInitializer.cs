using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GramKar.Models.Entities;
using GramKar.Services;

namespace GramKar.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Users if none exist
            if (!await context.Users.AnyAsync())
            {
                var hasher = new PasswordHasher<User>();

                var admin = new User
                {
                    Username = "admin",
                    FullName = "હસમુખભાઈ પરમાર (તલાટી કમ મંત્રી)",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

                var sarpanch = new User
                {
                    Username = "sarpanch",
                    FullName = "શ્રી રમેશભાઈ પટેલ (સરપંચ)",
                    Role = "Sarpanch",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                sarpanch.PasswordHash = hasher.HashPassword(sarpanch, "Sarpanch@123");

                context.Users.AddRange(admin, sarpanch);
                await context.SaveChangesAsync();
            }

            // 2. Seed Panchayat Setting
            if (!await context.PanchayatSettings.AnyAsync())
            {
                var setting = new PanchayatSetting
                {
                    PanchayatName = "ગ્રામ પંચાયત આણંદપર (Gram Panchayat Anandpar)",
                    VillageName = "આણંદપર (Anandpar)",
                    Taluka = "રાજકોટ (Rajkot)",
                    District = "રાજકોટ (Rajkot)",
                    SarpanchName = "શ્રી રમેશભાઈ પટેલ (Shri Rameshbhai Patel)",
                    SarpanchMessage = "ગામનો વિકાસ, સૌનો સાથ. સમયસર કર ભરી આપણા ગામને સ્વચ્છ, સુંદર અને સુવિધાસભર બનાવવામાં સહયોગ આપો.",
                    SarpanchPhotoPath = "/img/sarpanch.svg",
                    OfficeAddress = "પંચાયત ભવન, મુખ્ય બજાર, આણંદપર, જિ. રાજકોટ - 360003",
                    OfficeTimings = "સોમ થી શનિ: સવારે 10:00 થી સાંજે 5:00",
                    OfficePhone = "+91 98250 12345",
                    CurrentFinancialYear = "2025-26"
                };
                context.PanchayatSettings.Add(setting);
                await context.SaveChangesAsync();
            }

            // 3. Seed Tax Types
            if (!await context.TaxTypes.AnyAsync())
            {
                var taxTypes = new List<TaxType>
                {
                    new TaxType
                    {
                        Name = "ઘરવેરો (House Tax)",
                        DefaultAmount = 600m,
                        DueDay = 31,
                        DueMonth = 3,
                        LateFeeType = "Percent",
                        LateFeeValue = 2.0m, // 2% per month
                        MaxLateFeePercent = 50.0m,
                        AppliesTo = "All",
                        IsActive = true
                    },
                    new TaxType
                    {
                        Name = "પાણીવેરો (Water Tax)",
                        DefaultAmount = 400m,
                        DueDay = 31,
                        DueMonth = 3,
                        LateFeeType = "Percent",
                        LateFeeValue = 2.0m,
                        MaxLateFeePercent = 50.0m,
                        AppliesTo = "Water",
                        IsActive = true
                    },
                    new TaxType
                    {
                        Name = "સફાઈ અને દિવાબત્તી (Sanitation & Light)",
                        DefaultAmount = 250m,
                        DueDay = 31,
                        DueMonth = 3,
                        LateFeeType = "Flat",
                        LateFeeValue = 50.0m, // Flat ₹50 late fee
                        MaxLateFeePercent = 0m,
                        AppliesTo = "All",
                        IsActive = true
                    },
                    new TaxType
                    {
                        Name = "વ્યાપારી દુકાન કર (Commercial Shop Tax)",
                        DefaultAmount = 1200m,
                        DueDay = 31,
                        DueMonth = 3,
                        LateFeeType = "Percent",
                        LateFeeValue = 2.0m,
                        MaxLateFeePercent = 50.0m,
                        AppliesTo = "Shop",
                        IsActive = true
                    }
                };

                context.TaxTypes.AddRange(taxTypes);
                await context.SaveChangesAsync();
            }

            // 4. Seed 30 realistic households across 4 wards
            if (!await context.Households.AnyAsync())
            {
                var sampleHouseholds = new List<Household>
                {
                    // Ward 1: પટેલ ફળિયું (Patel Faliya)
                    new() { HouseNo = "PF-01", OwnerName = "જયંતિભાઈ કાનજીભાઈ પટેલ", GuardianName = "કાનજીભાઈ પટેલ", Mobile = "9825101001", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-02", OwnerName = "પ્રવીણભાઈ નરસીભાઈ પટેલ", GuardianName = "નરસીભાઈ પટેલ", Mobile = "9825101002", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-03", OwnerName = "મનસુખભાઈ શામજીભાઈ પટેલ", GuardianName = "શામજીભાઈ પટેલ", Mobile = "9825101003", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = false, IsActive = true },
                    new() { HouseNo = "PF-04", OwnerName = "દિનેશભાઈ બાબુભાઈ પટેલ", GuardianName = "બાબુભાઈ પટેલ", Mobile = "9825101004", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-05", OwnerName = "પટેલ કિરણ કુમાર વિઠ્ઠલભાઈ", GuardianName = "વિઠ્ઠલભાઈ પટેલ", Mobile = "9825101005", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-06", OwnerName = "ઉમિયા પ્રોવિઝન સ્ટોર (જગદીશભાઈ)", GuardianName = "મોહનભાઈ પટેલ", Mobile = "9825101006", Ward = "પટેલ ફળિયું", PropertyType = "Shop", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-07", OwnerName = "ભરતભાઈ નારણભાઈ પટેલ", GuardianName = "નારણભાઈ પટેલ", Mobile = "9825101007", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "PF-08", OwnerName = "હરસુખભાઈ જીવરાજભાઈ પટેલ", GuardianName = "જીવરાજભાઈ પટેલ", Mobile = "9825101008", Ward = "પટેલ ફળિયું", PropertyType = "Residential", HasWater = true, IsActive = true },

                    // Ward 2: ગામ તળાવ (Gam Talav)
                    new() { HouseNo = "GT-01", OwnerName = "ગોવિંદભાઈ રામાભાઈ ભરવાડ", GuardianName = "રામાભાઈ ભરવાડ", Mobile = "9825102001", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "GT-02", OwnerName = "કાળુભાઈ ભીખાભાઈ રબારી", GuardianName = "ભીખાભાઈ રબારી", Mobile = "9825102002", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = false, IsActive = true },
                    new() { HouseNo = "GT-03", OwnerName = "મેરામભાઈ દેવાભાઈ આહીર", GuardianName = "દેવાભાઈ આહીર", Mobile = "9825102003", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "GT-04", OwnerName = "ચોસલા ગોપાલભાઈ લાખાભાઈ", GuardianName = "લાખાભાઈ", Mobile = "9825102004", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "GT-05", OwnerName = "જય ગોપાલ ડેરી ફાર્મ (હમીરભાઈ)", GuardianName = "નારણભાઈ", Mobile = "9825102005", Ward = "ગામ તળાવ", PropertyType = "Shop", HasWater = true, IsActive = true },
                    new() { HouseNo = "GT-06", OwnerName = "વેલાભાઈ પુંજાભાઈ ભરવાડ", GuardianName = "પુંજાભાઈ ભરવાડ", Mobile = "9825102006", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "GT-07", OwnerName = "રામસીભાઈ જીવાભાઈ રબારી", GuardianName = "જીવાભાઈ રબારી", Mobile = "9825102007", Ward = "ગામ તળાવ", PropertyType = "Residential", HasWater = false, IsActive = true },

                    // Ward 3: નવા વાસ (Nava Vas)
                    new() { HouseNo = "NV-01", OwnerName = "મહેશભાઈ બાબુભાઈ પરમાર", GuardianName = "બાબુભાઈ પરમાર", Mobile = "9825103001", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-02", OwnerName = "રમેશભાઈ જેઠાભાઈ વાઘેલા", GuardianName = "જેઠાભાઈ વાઘેલા", Mobile = "9825103002", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-03", OwnerName = "કિશોરભાઈ નાથાભાઈ ચૌહાણ", GuardianName = "નાથાભાઈ ચૌહાણ", Mobile = "9825103003", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-04", OwnerName = "શારદાબેન કેશવજીભાઈ સોલંકી", GuardianName = "સ્વ. કેશવજીભાઈ", Mobile = "9825103004", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-05", OwnerName = "ચામુંડા ટેલર્સ (અશોકભાઈ)", GuardianName = "ચકુભાઈ વાઘેલા", Mobile = "9825103005", Ward = "નવા વાસ", PropertyType = "Shop", HasWater = false, IsActive = true },
                    new() { HouseNo = "NV-06", OwnerName = "ધીરુભાઈ રાઘવભાઈ મકવાણા", GuardianName = "રાઘવભાઈ મકવાણા", Mobile = "9825103006", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-07", OwnerName = "રાજેશભાઈ વસંતભાઈ રાઠોડ", GuardianName = "વસંતભાઈ રાઠોડ", Mobile = "9825103007", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "NV-08", OwnerName = "વિનોદભાઈ કરસનભાઈ ડાભી", GuardianName = "કરસનભાઈ ડાભી", Mobile = "9825103008", Ward = "નવા વાસ", PropertyType = "Residential", HasWater = true, IsActive = true },

                    // Ward 4: મંદિર ચોક (Mandir Chowk)
                    new() { HouseNo = "MC-01", OwnerName = "ભાનુશંકર પ્રભાશંકર જોશી", GuardianName = "પ્રભાશંકર જોશી", Mobile = "9825104001", Ward = "મંદિર ચોક", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "MC-02", OwnerName = "શાંતિલાલ મોહનલાલ શાહ", GuardianName = "મોહનલાલ શાહ", Mobile = "9825104002", Ward = "મંદિર ચોક", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "MC-03", OwnerName = "મહાવીર મેડિકલ સ્ટોર (ભાવિન શાહ)", GuardianName = "શાંતિલાલ શાહ", Mobile = "9825104003", Ward = "મંદિર ચોક", PropertyType = "Shop", HasWater = true, IsActive = true },
                    new() { HouseNo = "MC-04", OwnerName = "શ્રી રામ હાર્ડવેર (નરેશભાઈ સોની)", GuardianName = "હીરાલાલ સોની", Mobile = "9825104004", Ward = "મંદિર ચોક", PropertyType = "Shop", HasWater = false, IsActive = true },
                    new() { HouseNo = "MC-05", OwnerName = "હરેશભાઈ હરિલાલ ત્રિવેદી", GuardianName = "હરિલાલ ત્રિવેદી", Mobile = "9825104005", Ward = "મંદિર ચોક", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "MC-06", OwnerName = "ચંદ્રકાંત મનહરલાલ પંડ્યા", GuardianName = "મનહરલાલ પંડ્યા", Mobile = "9825104006", Ward = "મંદિર ચોક", PropertyType = "Residential", HasWater = true, IsActive = true },
                    new() { HouseNo = "MC-07", OwnerName = "શિવમ ટી સ્ટોલ (કાંતિલાલ)", GuardianName = "જેઠાલાલ પંડ્યા", Mobile = "9825104007", Ward = "મંદિર ચોક", PropertyType = "Shop", HasWater = true, IsActive = true }
                };

                context.Households.AddRange(sampleHouseholds);
                await context.SaveChangesAsync();
            }

            // 5. Generate Demands for FY 2024-25 and FY 2025-26
            if (!await context.Demands.AnyAsync())
            {
                var generator = new DemandGenerator(context);
                await generator.GenerateYearlyDemandsAsync("2024-25");
                await generator.GenerateYearlyDemandsAsync("2025-26");

                // Adjust due date for 2024-25 demands to be in the past (e.g. 31 Mar 2025)
                // so that overdue and late fees are realistically present
                var oldDemands = await context.Demands
                    .Where(d => d.FinancialYear == "2024-25")
                    .ToListAsync();

                foreach (var d in oldDemands)
                {
                    d.DueDate = new DateTime(2025, 3, 31);
                }

                // For 2025-26, due date is 31 Mar 2026 (also in the past relative to October 2026, creating current defaulters)
                var currentDemands = await context.Demands
                    .Where(d => d.FinancialYear == "2025-26")
                    .ToListAsync();

                foreach (var d in currentDemands)
                {
                    d.DueDate = new DateTime(2026, 3, 31);
                }

                await context.SaveChangesAsync();

                // 6. Record sample payments for several households
                var duesCalculator = new DuesCalculator();
                var paymentService = new PaymentService(context, duesCalculator);
                var adminUser = await context.Users.FirstAsync(u => u.Role == "Admin");
                var payDate = DateTime.Today;

                // Sample Payment 1: PF-01 pays in full for 2024-25
                var house1 = await context.Households.FirstAsync(h => h.HouseNo == "PF-01");
                var summary1 = duesCalculator.CalculateHouseholdSummary(house1, payDate);
                if (summary1.TotalPayable > 0)
                {
                    await paymentService.RecordPaymentAsync(
                        house1.HouseId,
                        summary1.TotalPayable,
                        "UPI",
                        "UPI/20261015/09812",
                        adminUser.UserId,
                        payDate);
                }

                // Sample Payment 2: GT-01 pays partial payment of ₹500
                var house2 = await context.Households.FirstAsync(h => h.HouseNo == "GT-01");
                await paymentService.RecordPaymentAsync(
                    house2.HouseId,
                    500m,
                    "Cash",
                    null,
                    adminUser.UserId,
                    payDate);

                // Sample Payment 3: NV-01 pays ₹800
                var house3 = await context.Households.FirstAsync(h => h.HouseNo == "NV-01");
                await paymentService.RecordPaymentAsync(
                    house3.HouseId,
                    800m,
                    "Cash",
                    null,
                    adminUser.UserId,
                    payDate);

                // Sample Payment 4: MC-03 (Medical Store) pays full via Cheque
                var house4 = await context.Households.FirstAsync(h => h.HouseNo == "MC-03");
                var summary4 = duesCalculator.CalculateHouseholdSummary(house4, payDate);
                if (summary4.TotalPayable > 0)
                {
                    await paymentService.RecordPaymentAsync(
                        house4.HouseId,
                        summary4.TotalPayable,
                        "Cheque",
                        "CHQ-581902-SBI",
                        adminUser.UserId,
                        payDate);
                }
            }
        }
    }
}
