using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeHavenApp.Helpers;
using CoffeeHavenApp.Services;
using CoffeeHavenApp.Testing.Base;
using CoffeeHavenDB.DAL;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenApp.Testing.Suites
{
    public class ProductTestSuite : TestSuite
    {
        private IProductService _productService;
        private InMemoryProductDAL _productRepo;

        public ProductTestSuite()
        {
            SuiteName = "Product Module (CRUD & Advanced Search/Filter)";
        }

        public override void GlobalSetup()
        {
            _productRepo = new InMemoryProductDAL();
            _productService = new ProductService(_productRepo);
            
            _testCases.Clear();
            _testCases.Add(new ProductCRUDTestCase(_productService));
            _testCases.Add(new SearchLogicTestCase(_productService));
            _testCases.Add(new FilterLogicTestCase(_productService));
            _testCases.Add(new SortLogicTestCase(_productService));
        }

        public override void Setup()
        {
            _productRepo.Reset();
        }

        private class ProductCRUDTestCase : ITestCase
        {
            private IProductService _service;
            public string Name => "Admin: Product CRUD Operations";
            public string ErrorMessage { get; private set; }

            public ProductCRUDTestCase(IProductService service) => _service = service;

            public bool Run()
            {
                _service.AddProduct(new Product { ProductName = "Americano", Price = 200, StockQuantity = 50 });
                var p = _service.GetAllProducts().FirstOrDefault(x => x.ProductName == "Americano");
                if (p == null) { ErrorMessage = "AddProduct failed."; return false; }

                p.Price = 250;
                _service.UpdateProduct(p.ProductId, p);
                if (_service.GetProductById(p.ProductId).Price != 250) { ErrorMessage = "UpdateProduct failed."; return false; }

                _service.DeleteProduct(p.ProductId);
                if (_service.GetProductById(p.ProductId) != null) { ErrorMessage = "DeleteProduct failed."; return false; }

                return true;
            }
        }

        private class SearchLogicTestCase : ITestCase
        {
            private IProductService _service;
            public string Name => "Customer: keyword Search Logic";
            public string ErrorMessage { get; private set; }

            public SearchLogicTestCase(IProductService service) => _service = service;

            public bool Run()
            {
                _service.AddProduct(new Product { ProductName = "Caramel Latte", Description = "Sweet coffee", Price = 300 });
                _service.AddProduct(new Product { ProductName = "Green Tea", Description = "Healthy drink", Price = 200 });

                var all = _service.GetAllProducts();
                var res1 = SearchHelper.SearchProducts(all, "Caramel");
                if (res1.Count != 1) { ErrorMessage = "Name match search failed."; return false; }

                var res2 = SearchHelper.SearchProducts(all, "Healthy");
                if (res2.Count != 1) { ErrorMessage = "Description match search failed."; return false; }

                return true;
            }
        }

        private class FilterLogicTestCase : ITestCase
        {
            private IProductService _service;
            public string Name => "Customer: Price & Stock Filtering";
            public string ErrorMessage { get; private set; }

            public FilterLogicTestCase(IProductService service) => _service = service;

            public bool Run()
            {
                _service.AddProduct(new Product { ProductName = "A", Price = 10, StockQuantity = 5 });
                _service.AddProduct(new Product { ProductName = "B", Price = 100, StockQuantity = 0 });
                _service.AddProduct(new Product { ProductName = "C", Price = 1000, StockQuantity = 10 });

                var all = _service.GetAllProducts();
                var filterPrice = SearchHelper.FilterByPriceRange(all, 0, 50);
                if (filterPrice.Count != 1 || filterPrice[0].ProductName != "A") { ErrorMessage = "Price filter failed."; return false; }

                var filterStock = SearchHelper.FilterInStock(all);
                if (filterStock.Any(x => x.ProductName == "B")) { ErrorMessage = "Stock filter failed; included out of stock."; return false; }

                return true;
            }
        }

        private class SortLogicTestCase : ITestCase
        {
            private IProductService _service;
            public string Name => "Customer: Multi-Column Sorting";
            public string ErrorMessage { get; private set; }

            public SortLogicTestCase(IProductService service) => _service = service;

            public bool Run()
            {
                _service.AddProduct(new Product { ProductName = "Z", Price = 100 });
                _service.AddProduct(new Product { ProductName = "A", Price = 500 });
                
                var all = _service.GetAllProducts();
                var sort1 = SearchHelper.SortProducts(all, "name", true);
                if (sort1[0].ProductName != "A") { ErrorMessage = "Alphabetical sort failed."; return false; }

                var sort2 = SearchHelper.SortProducts(all, "price", false);
                if (sort2[0].ProductName != "A") { ErrorMessage = "Price descending sort failed."; return false; }

                return true;
            }
        }
    }
}
