using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenApp.Testing.Mocks
{
    public class InMemoryOrderDAL : IOrderRepository
    {
        private List<OrderMock> _orders = new List<OrderMock>();
        private int _nextId = 1;

        public void Reset()
        {
            _orders.Clear();
            _nextId = 1;
        }

        public void PlaceOrder(int userId, int itemId, int quantity, string paymentMethod)
        {
            _orders.Add(new OrderMock
            {
                OrderId = _nextId++,
                UserId = userId,
                ItemId = itemId,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                Status = "Completed",
                PaymentMethod = paymentMethod
            });
        }

        public void PlaceOrder(int userId, Dictionary<int, int> itemsToOrder, string paymentMethod)
        {
            foreach (var item in itemsToOrder)
            {
                PlaceOrder(userId, item.Key, item.Value, paymentMethod);
            }
        }

        public void CancelOrder(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = "Cancelled";
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null) order.Status = status;
        }

        public void ClearUserOrderHistory(int userId)
        {
            _orders.RemoveAll(o => o.UserId == userId);
        }

        public DataTable GetUserOrderHistory(int userId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("OrderID", typeof(int));
            dt.Columns.Add("ItemID", typeof(int));
            dt.Columns.Add("Quantity", typeof(int));
            dt.Columns.Add("OrderDate", typeof(DateTime));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("PaymentMethod", typeof(string));
            dt.Columns.Add("ProductName", typeof(string)); 
            dt.Columns.Add("UnitPrice", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));

            foreach (var o in _orders.Where(o => o.UserId == userId))
            {
                dt.Rows.Add(o.OrderId, o.ItemId, o.Quantity, o.OrderDate, o.Status, o.PaymentMethod, "Mock Product", 5.0m, 5.0m * o.Quantity);
            }
            return dt;
        }

        public DataTable GetAllOrders()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("OrderID", typeof(int));
            dt.Columns.Add("Customer", typeof(string));
            dt.Columns.Add("OrderDate", typeof(DateTime));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("PaymentMethod", typeof(string));
            dt.Columns.Add("GrantTotal", typeof(decimal));

            foreach (var o in _orders)
            {
                dt.Rows.Add(o.OrderId, $"User {o.UserId}", o.OrderDate, o.Status, o.PaymentMethod, 50.0m);
            }
            return dt;
        }

        private class OrderMock
        {
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public string PaymentMethod { get; set; }
        }
    }
}
