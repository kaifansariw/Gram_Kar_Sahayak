using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GramKar.Models.Entities
{
    public class PaymentAllocation
    {
        [Key]
        public int AllocId { get; set; }

        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public int DemandId { get; set; }
        public Demand? Demand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeePaid { get; set; }
    }
}
