using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleID { get; set; }

        [ForeignKey("Driver")]
        public int DriverID { get; set; }
        public Driver Driver { get; set; }

        [ForeignKey("VehicleType")]
        public int VehicleTypeID { get; set; }
        public VehicleTypes VehicleType { get; set; }

        [StringLength(100)]
        public string VehicleMake { get; set; }

        [StringLength(100)]
        public string VehicleModel { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleNumberPlate { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
