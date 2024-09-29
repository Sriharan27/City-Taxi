using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class VehicleTypes
    {
        [Key]
        public int VehicleTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; }

        public int? PassengerCapacity { get; set; }

        public decimal PricePerKM { get; set; }
    }
}
