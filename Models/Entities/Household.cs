using System.ComponentModel.DataAnnotations;

namespace GramKar.Models.Entities
{
    public class Household
    {
        [Key]
        public int HouseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HouseNo { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string OwnerName { get; set; } = string.Empty;

        [MaxLength(120)]
        public string GuardianName { get; set; } = string.Empty; // Father/Husband Name

        [MaxLength(15)]
        public string? Mobile { get; set; }

        [Required]
        [MaxLength(60)]
        public string Ward { get; set; } = string.Empty; // Ward / Faliya

        [Required]
        [MaxLength(30)]
        public string PropertyType { get; set; } = "Residential"; // Residential / Shop / Other

        public bool HasWater { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Demand> Demands { get; set; } = new List<Demand>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
