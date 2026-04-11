using System.Collections.Generic;
using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    // Part of Lab 6: Contract for Order Data Access
    public interface IOrderRepository
    {
        void PlaceOrder(int userId, int itemId, int quantity, string paymentMethod);
        void PlaceOrder(int userId, Dictionary<int, int> itemsToOrder, string paymentMethod);
        void CancelOrder(int orderId);
        void UpdateOrderStatus(int orderId, string status);
        void ClearUserOrderHistory(int userId);
        DataTable GetUserOrderHistory(int userId);
        DataTable GetAllOrders(); // For Admin
    }
}