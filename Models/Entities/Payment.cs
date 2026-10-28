using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GramKar.Models.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [MaxLength(40)]
        public string ReceiptNo { get; set; } = string.Empty; // e.g. "GP-2025-000123"

        public int HouseId { get; set; }
        public Household? Household { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Mode { get; set; } = "Cash"; // Cash / UPI / Cheque

        [MaxLength(80)]
        public string? Reference { get; set; } // Cheque no. or UPI txn ref

        public int ReceivedByUserId { get; set; }
        public User? ReceivedByUser { get; set; }

        public bool IsCancelled { get; set; } = false;

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    }
}
