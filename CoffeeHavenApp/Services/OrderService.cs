using System;
using System.Collections.Generic;
using System.Data;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenApp.Services
{
    /// <summary>
    /// Lab 07 Implementation: OrderService BLL
    /// Orchestrates logic between ProductService (for stock checks) and OrderRepository.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService; // Dependency for stock verification

        /// <summary>
        /// Constructor injection of both Order Repository and Product Service
        /// </summary>
        public OrderService(IOrderRepository orderRepository, IProductService productService)
        {
            _orderRepository = orderRepository;
            _productService = productService;
        }

        /// <summary>
        /// Place a single item order with inventory validation.
        /// </summary>
        public void PlaceOrder(int userId, int itemId, int quantity, string paymentMethod)
        {
            Console.WriteLine($"[BLL] Order Request: User {userId} wants {quantity} of Item {itemId}");

            // 1. [LOGIC] Check stock availability via ProductService
            if (_productService.IsProductAvailable(itemId, quantity))
            {
                // 2. [BLL] Instruct ProductService to reduce stock
                _productService.ReduceStock(itemId, quantity);

                // 3. [DAL] Proceed to save the order record
                _orderRepository.PlaceOrder(userId, itemId, quantity, paymentMethod);

                Console.WriteLine("[BLL] Transaction Complete: Order placed and stock adjusted.");
            }
            else
            {
                Console.WriteLine("[BLL] ERROR: Order Rejected. Insufficient stock available.");
            }
        }

        /// <summary>
        /// Place a multi-item order (Shopping Cart) with "All-or-Nothing" validation.
        /// </summary>
        public void PlaceOrder(int userId, Dictionary<int, int> itemsToOrder, string paymentMethod)
        {
            Console.WriteLine("[BLL] Processing Bulk Order Request...");
            bool canProceed = true;

            // 1. [LOGIC] Validate the ENTIRE cart before processing
            foreach (var item in itemsToOrder)
            {
                if (!_productService.IsProductAvailable(item.Key, item.Value))
                {
                    Console.WriteLine($"[BLL] ERROR: Item {item.Key} does not have {item.Value} units in stock.");
                    canProceed = false;
                    break;
                }
            }

            if (canProceed)
            {
                // 2. [BLL] Deduct stock for every item in the cart
                foreach (var item in itemsToOrder)
                {
                    _productService.ReduceStock(item.Key, item.Value);
                }

                // 3. [DAL] Record the bulk transaction
                _orderRepository.PlaceOrder(userId, itemsToOrder, paymentMethod);
                Console.WriteLine("[BLL] Bulk Transaction Complete.");
            }
            else
            {
                Console.WriteLine("[BLL] ERROR: Entire order cancelled due to stock shortages.");
            }
        }

        public void CancelOrder(int orderId)
        {
            Console.WriteLine($"[BLL] Processing Cancellation for Order #{orderId}");

            // Note: In a production system, we would fetch the order details here 
            // and call _productService.Restock() for the items.
            // For Lab 07, the DAL handles the SQL-side restoration.
            _orderRepository.CancelOrder(orderId);

            Console.WriteLine("[BLL] Order cancellation logic completed.");
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            Console.WriteLine($"[BLL] Updating Order #{orderId} to status: {status}");
            _orderRepository.UpdateOrderStatus(orderId, status);
        }

        public void ClearUserOrderHistory(int userId)
        {
            Console.WriteLine($"[BLL] Clearing history for User {userId}");
            _orderRepository.ClearUserOrderHistory(userId);
        }

        public DataTable GetUserOrderHistory(int userId)
        {
            return _orderRepository.GetUserOrderHistory(userId);
        }

        public DataTable GetAllOrders()
        {
            return _orderRepository.GetAllOrders();
        }
    }
}