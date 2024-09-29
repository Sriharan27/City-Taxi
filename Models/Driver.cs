using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Driver
    {
        [Key]
        public int DriverID { get; set; }

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

        [Required]
        [StringLength(20)]
        public string NIC { get; set; }

        public byte[] Image { get; set; } // Stores the driver's image as binary data

        public DateTime DateRegistered { get; set; } = DateTime.Now;

        [StringLength(10)]
        public string BankCode { get; set; }

        public int BranchID { get; set; }

        [StringLength(20)]
        public string AccountNumber { get; set; }

        [ForeignKey("BranchID")]
        public BankBranches BankBranches { get; set; }
    }
}
