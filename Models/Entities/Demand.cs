using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GramKar.Models.Entities
{
    public class Demand
    {
        [Key]
        public int DemandId { get; set; }

        public int HouseId { get; set; }
        public Household? Household { get; set; }

        public int TaxTypeId { get; set; }
        public TaxType? TaxType { get; set; }

        [Required]
        [MaxLength(20)]
        public string FinancialYear { get; set; } = string.Empty; // e.g. "2025-26"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Principal assessment amount

        public DateTime DueDate { get; set; }

        [MaxLength(250)]
        public string? Remarks { get; set; } // Concession reason or adjustment note

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    }
}
