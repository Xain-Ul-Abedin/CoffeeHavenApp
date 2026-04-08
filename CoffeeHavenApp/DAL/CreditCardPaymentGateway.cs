using System;
using CoffeeHaven.Interfaces;
using CoffeeHaven.Models;

namespace CoffeeHaven.DAL
{
    public class CreditCardPaymentGateway : IPaymentGateway
    {
        public bool ProcessPayment(Payment payment)
        {
            Console.WriteLine("\n[GATEWAY] Contacting Bank via Secure Channel...");

            // Mock Validation
            if (string.IsNullOrEmpty(payment.CardNumber) || payment.CardNumber.Length < 12)
            {
                Console.WriteLine("[ERROR] Invalid Credit Card Number.");
                return false;
            }

            Console.WriteLine($"[SUCCESS] Authorized ${payment.Amount} via Credit Card (****{payment.CardNumber.Substring(payment.CardNumber.Length - 4)})");
            return true;
        }
    }
}