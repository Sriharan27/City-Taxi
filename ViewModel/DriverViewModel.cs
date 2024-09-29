using System.ComponentModel.DataAnnotations;
using City_Taxi.Models;
using Microsoft.AspNetCore.Http;

namespace City_Taxi.ViewModel
{
    public class DriverViewModel
    {
        [Required]
        [StringLength(255)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(255)]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string NIC { get; set; }

        public IFormFile ImageFile { get; set; }

        [StringLength(10)]
        public string BankCode { get; set; }

        public int BranchID { get; set; }

        [StringLength(20)]
        public string AccountNumber { get; set; }
        public List<Banks> Banks { get; set; } // Assuming you have a Bank model
        public List<BankBranches> Branches { get; set; } // Assuming you have a Branch model
    }
}
