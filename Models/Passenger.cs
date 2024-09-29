using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Passenger
    {
        [Key]
        public int PassengerID { get; set; }

        [Required]
        [StringLength(255)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(255)]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public DateTime DateRegistered { get; set; } = DateTime.Now;

        public string CurrentLatitude { get; set; } = "null";
        public string CurrentLongitude { get; set; } = "null";
    }
}
