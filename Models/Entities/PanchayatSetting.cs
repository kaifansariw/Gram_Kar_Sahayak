using System.ComponentModel.DataAnnotations;

namespace GramKar.Models.Entities
{
    public class PanchayatSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PanchayatName { get; set; } = "ગ્રામ પંચાયત આણંદપર (Gram Panchayat Anandpar)";

        [Required]
        [MaxLength(80)]
        public string VillageName { get; set; } = "આણંદપર (Anandpar)";

        [MaxLength(60)]
        public string Taluka { get; set; } = "રાજકોટ (Rajkot)";

        [MaxLength(60)]
        public string District { get; set; } = "રાજકોટ (Rajkot)";

        [Required]
        [MaxLength(100)]
        public string SarpanchName { get; set; } = "શ્રી રમેશભાઈ પટેલ (Shri Rameshbhai Patel)";

        [Required]
        [MaxLength(500)]
        public string SarpanchMessage { get; set; } = "ગામનો વિકાસ, સૌનો સાથ. સમયસર કર ભરી આપણા ગામને સ્વચ્છ, સુંદર અને સમૃદ્ધ બનાવવામાં સહયોગ આપો. (Development of our village with everyone's support. Pay dues timely to make our village clean, beautiful and prosperous.)";

        [MaxLength(200)]
        public string SarpanchPhotoPath { get; set; } = "/img/sarpanch.svg";

        [MaxLength(200)]
        public string OfficeAddress { get; set; } = "પંચાયત ભવન, મુખ્ય બજાર, આણંદપર - 360003";

        [MaxLength(100)]
        public string OfficeTimings { get; set; } = "સોમ થી શનિ: સવારે 10:00 થી સાંજે 5:00 (Mon to Sat: 10 AM to 5 PM)";

        [MaxLength(30)]
        public string OfficePhone { get; set; } = "+91 98250 12345";

        [MaxLength(20)]
        public string CurrentFinancialYear { get; set; } = "2025-26";
    }
}
