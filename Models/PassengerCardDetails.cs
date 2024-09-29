using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace City_Taxi.Models
{
    public class PassengerCardDetails
    {
        [Key]
        public int CardID { get; set; }

        [ForeignKey("Passenger")]
        public int PassengerID { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 16 digits.")]
        public string CardNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string CardHolderName { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Expiration date must be in MM/YY format.")]
        [StringLength(5, ErrorMessage = "Expiration date must be 5 characters long in MM/YY format.")]
        public string ExpirationDate { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "CVV must be stored in encrypted format.")]
        public string CVV { get; set; }

        // Navigation property
        public Passenger Passenger { get; set; }
    }
}

