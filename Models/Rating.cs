using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Rating
    {
        [Key]
        public int RatingID { get; set; }

        [ForeignKey("Passenger")]
        public int PassengerID { get; set; }
        public Passenger Passenger { get; set; }

        [ForeignKey("Driver")]
        public int DriverID { get; set; }
        public Driver Driver { get; set; }

        public int? RatingValue { get; set; }

        [StringLength(1000)]
        public string Review { get; set; }

        public DateTime DateReviewed { get; set; } = DateTime.Now;
    }
}
