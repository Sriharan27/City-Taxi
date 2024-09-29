using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Banks
    {
        [Key]
        [Required]
        [StringLength(10)]
        public string BankCode { get; set; }  // Primary key

        [Required]
        [StringLength(255)]
        public string BankName { get; set; }
    }
}
