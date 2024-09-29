using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class DriverStatus
    {
        [Key]
        public int DriverStatusID { get; set; }

        [ForeignKey("Driver")]
        public int DriverID { get; set; }
        public Driver Driver { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        public string Latitude { get; set; }

        [Required]
        public string Longitude { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
