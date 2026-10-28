using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GramKar.Models.Entities
{
    public class TaxType
    {
        [Key]
        public int TaxTypeId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Name { get; set; } = string.Empty; // e.g. House Tax, Water Tax, Sanitation, Street Light

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultAmount { get; set; }

        public int DueDay { get; set; } = 31; // Day of month

        public int DueMonth { get; set; } = 3; // Month (e.g. 3 for March)

        [Required]
        [MaxLength(20)]
        public string LateFeeType { get; set; } = "Percent"; // "Percent" or "Flat"

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeeValue { get; set; } = 2.0m; // 2% per month or Flat fee

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxLateFeePercent { get; set; } = 50.0m; // Cap e.g. 50% of principal

        [Required]
        [MaxLength(40)]
        public string AppliesTo { get; set; } = "All"; // "All", "Water", "Shop"

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Demand> Demands { get; set; } = new List<Demand>();
    }
}
