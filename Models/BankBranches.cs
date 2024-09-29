using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace City_Taxi.Models
{
    public class BankBranches
    {
        [Key]
        public int BranchID { get; set; }

        [Required]
        [StringLength(10)]
        public string BankCode { get; set; } 

        [Required]
        [StringLength(10)]
        public string BranchCode { get; set; }

        [Required]
        [StringLength(255)]
        public string BranchName { get; set; }

        [ForeignKey("BankCode")]
        public Banks Bank { get; set; }
    }
}
