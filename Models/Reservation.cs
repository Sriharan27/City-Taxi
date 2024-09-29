using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        [ForeignKey("Passenger")]
        public int PassengerID { get; set; }
        public Passenger Passenger { get; set; }

        [ForeignKey("Driver")]
        public int DriverID { get; set; }
        public Driver Driver { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleID { get; set; }
        public Vehicle Vehicle { get; set; }

        [Required]
        [StringLength(255)]
        public string PickupLocation { get; set; }

        [Required]
        [StringLength(255)]
        public string DropoffLocation { get; set; }

        [StringLength(255)]
        public string ReservationStatus { get; set; }

        [StringLength(20)]
        public string PaymentStatus { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; }

        public DateTime? PickupTime { get; set; }

        public DateTime? DropoffTime { get; set; }

        public double TripPrice {  get; set; }
        public double TripDistance { get; set; }
        public string PaymentType { get; set; }
        public int CardID { get; set; }
    }
}
