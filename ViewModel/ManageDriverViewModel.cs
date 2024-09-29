namespace City_Taxi.ViewModel
{
    public class ManageDriverViewModel
    {
        public int DriverID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string NIC { get; set; }
        public byte[] Image { get; set; } // Stores the driver's image as binary data
        public DateTime DateRegistered { get; set; }
        public string AccountNumber { get; set; }

        // Bank details
        public string BankName { get; set; }

        // Branch details
        public string BranchName { get; set; }
    }
}
