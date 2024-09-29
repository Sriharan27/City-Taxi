namespace City_Taxi.ViewModel
{
    public class PaymentMethodViewModel
    {
        public List<CardViewModel> AvailableCards { get; set; }
        public string SelectedPaymentMethod { get; set; }
    }
}
