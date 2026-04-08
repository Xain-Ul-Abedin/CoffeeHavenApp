using System;
using CoffeeHaven.Interfaces;
using CoffeeHaven.Models;

namespace CoffeeHaven.DAL
{
    public class PayPalPaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(Payment payment)
        {
            Console.WriteLine("\n[GATEWAY] Redirecting to PayPal Authorization...");

            if (string.IsNullOrEmpty(payment.PayPalEmail) || !payment.PayPalEmail.Contains("@"))
            {
                Console.WriteLine("[ERROR] Invalid PayPal Credentials.");
                return false;
            }

            Console.WriteLine($"[SUCCESS] Authorized ${payment.Amount} via PayPal Account: {payment.PayPalEmail}");
            return true;
        }
    }
}