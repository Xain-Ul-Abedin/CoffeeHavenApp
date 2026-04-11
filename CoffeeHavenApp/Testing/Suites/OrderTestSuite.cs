using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CoffeeHavenApp.Services;
using CoffeeHavenApp.Testing.Base;
using CoffeeHavenApp.Testing.Mocks;
using CoffeeHavenDB.DAL;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenApp.Testing.Suites
{
    public class OrderTestSuite : TestSuite
    {
        private IOrderService _orderService;
        private IProductService _productService;
        private InMemoryOrderDAL _orderRepo;
        private InMemoryProductDAL _productRepo;

        public OrderTestSuite()
        {
            SuiteName = "Order Module (Cart, Stock & History)";
        }

        public override void GlobalSetup()
        {
            _productRepo = new InMemoryProductDAL();
            _orderRepo = new InMemoryOrderDAL();
            _productService = new ProductService(_productRepo);
            _orderService = new OrderService(_orderRepo, _productService);

            _testCases.Clear();
            _testCases.Add(new SingleOrderTestCase(_orderService, _productService));
            _testCases.Add(new BulkOrderTestCase(_orderService, _productService));
            _testCases.Add(new OrderCancellationTestCase(_orderService, _productService));
            _testCases.Add(new HistoryVerificationTestCase(_orderService, _productService));
        }

        public override void Setup()
        {
            _productRepo.Reset();
            _orderRepo.Reset();
        }

        private class SingleOrderTestCase : ITestCase
        {
            private IOrderService _orderService;
            private IProductService _productService;
            public string Name => "Customer: Single Item Placement";
            public string ErrorMessage { get; private set; }

            public SingleOrderTestCase(IOrderService os, IProductService ps) { _orderService = os; _productService = ps; }

            public bool Run()
            {
                _productService.AddProduct(new Product { ProductName = "Espresso", Price = 100, StockQuantity = 10 });
                int id = _productService.GetAllProducts().First().ProductId;

                _orderService.PlaceOrder(1, id, 3, "Credit Card");

                var p = _productService.GetProductById(id);
                if (p.StockQuantity != 7) { ErrorMessage = $"Stock expected 7, got {p.StockQuantity}."; return false; }

                return true;
            }
        }

        private class BulkOrderTestCase : ITestCase
        {
            private IOrderService _orderService;
            private IProductService _productService;
            public string Name => "Customer: Bulk Order (All-or-Nothing)";
            public string ErrorMessage { get; private set; }

            public BulkOrderTestCase(IOrderService os, IProductService ps) { _orderService = os; _productService = ps; }

            public bool Run()
            {
                _productService.AddProduct(new Product { ProductName = "A", Price = 10, StockQuantity = 10 });
                _productService.AddProduct(new Product { ProductName = "B", Price = 10, StockQuantity = 1 });

                var all = _productService.GetAllProducts();
                int idA = all.First(x => x.ProductName == "A").ProductId;
                int idB = all.First(x => x.ProductName == "B").ProductId;

                var cart = new Dictionary<int, int> { { idA, 5 }, { idB, 2 } }; // B only has 1

                _orderService.PlaceOrder(1, cart, "PayPal");

                if (_productService.GetProductById(idA).StockQuantity != 10)
                {
                    ErrorMessage = "Stock for 'A' was reduced even though 'B' was unavailable.";
                    return false;
                }

                return true;
            }
        }

        private class OrderCancellationTestCase : ITestCase
        {
            private IOrderService _orderService;
            private IProductService _productService;
            public string Name => "Cancellation Logic";
            public string ErrorMessage { get; private set; }

            public OrderCancellationTestCase(IOrderService os, IProductService ps) { _orderService = os; _productService = ps; }

            public bool Run()
            {
                _productService.AddProduct(new Product { ProductName = "CancelMe", Price = 10, StockQuantity = 10 });
                int id = _productService.GetAllProducts().First().ProductId;

                _orderService.PlaceOrder(1, id, 5, "Credit Card");
                _orderService.CancelOrder(1); 

                DataTable history = _orderService.GetUserOrderHistory(1);
                if (history.Rows[0]["Status"].ToString() != "Cancelled")
                {
                    ErrorMessage = "Order status not updated to Cancelled.";
                    return false;
                }

                return true;
            }
        }

        private class HistoryVerificationTestCase : ITestCase
        {
            private IOrderService _orderService;
            private IProductService _productService;
            public string Name => "Order History Integrity";
            public string ErrorMessage { get; private set; }

            public HistoryVerificationTestCase(IOrderService os, IProductService ps) { _orderService = os; _productService = ps; }

            public bool Run()
            {
                _productService.AddProduct(new Product { ProductName = "P1", Price = 10, StockQuantity = 10 });
                int id = _productService.GetAllProducts().First().ProductId;

                _orderService.PlaceOrder(10, id, 1, "Credit Card");
                _orderService.PlaceOrder(20, id, 1, "PayPal");

                DataTable hist10 = _orderService.GetUserOrderHistory(10);
                if (hist10.Rows.Count != 1) { ErrorMessage = "Incorrect history count for User 10."; return false; }

                return true;
            }
        }
    }
}
