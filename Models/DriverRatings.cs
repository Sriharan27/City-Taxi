using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace City_Taxi.Models
{
    public class DriverRatings
    {
        [Key]
        public int DriverRatingID { get; set; }

        [Required]
        [ForeignKey("Reservation")]
        public int ReservationID { get; set; }
        public virtual Reservation Reservation { get; set; } 

        [Required]
        [Range(1, 5, ErrorMessage = "Please enter a rating between 1 and 5 stars.")]
        public int Stars { get; set; }
    }
}
