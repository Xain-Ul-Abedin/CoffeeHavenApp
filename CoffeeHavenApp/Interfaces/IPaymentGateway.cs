using CoffeeHaven.Models;

namespace CoffeeHaven.Interfaces
{
    public interface IPaymentGateway
    {
        bool ProcessPayment(Payment payment);
    }
}