using System.Collections.Generic;
using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    // Part of Lab 6: Contract for Order Business Logic
    public interface IOrderService
    {
        void PlaceOrder(int userId, int itemId, int quantity);
        void PlaceOrder(int userId, Dictionary<int, int> itemsToOrder);
        void CancelOrder(int orderId);
        DataTable GetUserOrderHistory(int userId);
    }
}