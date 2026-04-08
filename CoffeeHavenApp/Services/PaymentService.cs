using System;
using CoffeeHaven.Interfaces;
using CoffeeHaven.Models;

namespace CoffeeHaven.Services
{
    public class PaymentService
    {
        private readonly IPaymentGateway _paymentGateway;

        // Dependency Injection via Constructor
        public PaymentService(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public bool ExecutePayment(Payment payment)
        {
            Console.WriteLine("\n--- Processing Transaction ---");
            bool result = _paymentGateway.ProcessPayment(payment);

            if (result)
                Console.WriteLine("Result: Payment Approved.");
            else
                Console.WriteLine("Result: Payment Declined.");

            return result;
        }
    }
}