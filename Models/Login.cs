using System.ComponentModel.DataAnnotations;

namespace City_Taxi.Models
{
    public class Login
    {
        [Key]
        public int LoginID { get; set; }

        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        public string UserType { get; set; }

        [Required]
        public int UserID { get; set; }
    }
}
