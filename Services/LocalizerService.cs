using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace GramKar.Services
{
    public class LocalizerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalizerService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentLanguage
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Request.Cookies.TryGetValue("GramKar_Lang", out var lang))
                {
                    if (lang == "hi" || lang == "en" || lang == "gu")
                    {
                        return lang;
                    }
                }
                return "gu"; // Gujarati default
            }
        }

        private static readonly Dictionary<string, (string Gu, string Hi, string En)> Dictionary = LoadTranslations();

        private static Dictionary<string, (string Gu, string Hi, string En)> LoadTranslations()
        {
            var dict = new Dictionary<string, (string Gu, string Hi, string En)>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "translations.json");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "translations.json");
                }

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    using var doc = JsonDocument.Parse(json);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var gu = prop.Value.TryGetProperty("gu", out var g) ? g.GetString() ?? "" : "";
                        var hi = prop.Value.TryGetProperty("hi", out var h) ? h.GetString() ?? "" : "";
                        var en = prop.Value.TryGetProperty("en", out var e) ? e.GetString() ?? "" : "";
                        dict[prop.Name] = (gu, hi, en);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalizerService] Error loading translations: {ex.Message}");
            }
            return dict;
        }

        public string this[string key]
        {
            get
            {
                if (Dictionary.TryGetValue(key, out var val))
                {
                    return CurrentLanguage switch
                    {
                        "hi" => val.Hi,
                        "en" => val.En,
                        _ => val.Gu
                    };
                }
                return key;
            }
        }

        public string LocalizeTaxName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var n = name.Trim();

            if (n.Contains("ઘર") || n.Contains("મકાન") || n.Contains("House", StringComparison.OrdinalIgnoreCase))
            {
                return CurrentLanguage switch
                {
                    "hi" => "मकान कर",
                    "en" => "House Tax",
                    _ => "ઘરવેરો"
                };
            }
            if (n.Contains("પાણી") || n.Contains("Water", StringComparison.OrdinalIgnoreCase))
            {
                return CurrentLanguage switch
                {
                    "hi" => "पानी कर",
                    "en" => "Water Tax",
                    _ => "પાણીવેરો"
                };
            }
            if (n.Contains("દિવાબત્તી") || n.Contains("Light", StringComparison.OrdinalIgnoreCase) || n.Contains("રોશની"))
            {
                return CurrentLanguage switch
                {
                    "hi" => "प्रकाश कर",
                    "en" => "Lighting Tax",
                    _ => "દિવાબત્તી કર"
                };
            }
            if (n.Contains("સફાઈ") || n.Contains("Sanitation", StringComparison.OrdinalIgnoreCase) || n.Contains("सफाई"))
            {
                return CurrentLanguage switch
                {
                    "hi" => "सफाई कर",
                    "en" => "Sanitation Tax",
                    _ => "સફાઈ કર"
                };
            }
            if (n.Contains("દુકાન") || n.Contains("Shop", StringComparison.OrdinalIgnoreCase) || n.Contains("Commercial", StringComparison.OrdinalIgnoreCase))
            {
                return CurrentLanguage switch
                {
                    "hi" => "दुकान कर",
                    "en" => "Commercial Tax",
                    _ => "દુકાન વેરો"
                };
            }

            return n;
        }

        public string ConvertAmountToWords(long number)
        {
            if (number == 0)
            {
                return CurrentLanguage switch
                {
                    "hi" => "शून्य रुपये मात्र",
                    "en" => "Zero Rupees Only",
                    _ => "શૂન્ય રૂપિયા પૂરા"
                };
            }

            if (CurrentLanguage == "en")
            {
                return NumberToWordsEn(number).Trim() + " Rupees Only";
            }
            else if (CurrentLanguage == "hi")
            {
                return NumberToWordsHi(number).Trim() + " रुपये मात्र";
            }
            else
            {
                return NumberToWordsGu(number).Trim() + " રૂપિયા પૂરા";
            }
        }

        private static string NumberToWordsEn(long n)
        {
            if (n == 0) return "";
            if (n >= 10000000) return (NumberToWordsEn(n / 10000000) + " Crore " + NumberToWordsEn(n % 10000000)).Trim();
            if (n >= 100000) return (NumberToWordsEn(n / 100000) + " Lakh " + NumberToWordsEn(n % 100000)).Trim();
            if (n >= 1000) return (NumberToWordsEn(n / 1000) + " Thousand " + NumberToWordsEn(n % 1000)).Trim();
            if (n >= 100) return (NumberToWordsEn(n / 100) + " Hundred " + NumberToWordsEn(n % 100)).Trim();

            var units = new[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                                "Seventeen", "Eighteen", "Nineteen" };
            var tens = new[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (n < 20) return units[n];
            return (tens[n / 10] + " " + units[n % 10]).Trim();
        }

        private static string NumberToWordsHi(long n)
        {
            if (n == 0) return "";
            if (n >= 10000000) return (NumberToWordsHi(n / 10000000) + " करोड़ " + NumberToWordsHi(n % 10000000)).Trim();
            if (n >= 100000) return (NumberToWordsHi(n / 100000) + " लाख " + NumberToWordsHi(n % 100000)).Trim();
            if (n >= 1000) return (NumberToWordsHi(n / 1000) + " हज़ार " + NumberToWordsHi(n % 1000)).Trim();
            if (n >= 100) return (NumberToWordsHi(n / 100) + " सौ " + NumberToWordsHi(n % 100)).Trim();

            var units = new[] { "", "एक", "दो", "तीन", "चार", "पाँच", "छह", "सात", "आठ", "नौ",
                                "दस", "ग्यारह", "बारह", "तेरह", "चौदह", "पंद्रह", "सोलह",
                                "सत्रह", "अठारह", "उन्नीस" };
            var tens = new[] { "", "", "बीस", "तीस", "चालीस", "पचास", "साठ", "सत्तर", "अस्सी", "नब्बे" };

            if (n < 20) return units[n];
            return (tens[n / 10] + " " + units[n % 10]).Trim();
        }

        private static string NumberToWordsGu(long n)
        {
            if (n == 0) return "";
            if (n >= 10000000) return (NumberToWordsGu(n / 10000000) + " કરોડ " + NumberToWordsGu(n % 10000000)).Trim();
            if (n >= 100000) return (NumberToWordsGu(n / 100000) + " લાખ " + NumberToWordsGu(n % 100000)).Trim();
            if (n >= 1000) return (NumberToWordsGu(n / 1000) + " હજાર " + NumberToWordsGu(n % 1000)).Trim();
            if (n >= 100) return (NumberToWordsGu(n / 100) + " સો " + NumberToWordsGu(n % 100)).Trim();

            var units = new[] { "", "એક", "બે", "ત્રણ", "ચાર", "પાંચ", "છ", "સાત", "આઠ", "નવ",
                                "દસ", "અગિયાર", "બાર", "તેર", "ચૌદ", "પંદર", "સોળ",
                                "સત્તર", "અઢાર", "ઓગણીસ" };
            var tens = new[] { "", "", "વીસ", "ત્રીસ", "ચાલીસ", "પચાસ", "સાઠ", "સિત્તેર", "એંસી", "નેવું" };

            if (n < 20) return units[n];
            return (tens[n / 10] + " " + units[n % 10]).Trim();
        }
    }
}
