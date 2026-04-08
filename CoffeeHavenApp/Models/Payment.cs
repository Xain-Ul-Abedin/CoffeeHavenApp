namespace CoffeeHaven.Models
{
    public class Payment
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // "Credit Card" or "PayPal"

        // Credit Card Details
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }

        // PayPal Details
        public string PayPalEmail { get; set; }
    }
}