using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("Reservation")]
        public int ReservationID { get; set; }
        public Reservation Reservation { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaymentAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string PaymentStatus { get; set; }
    }
}
