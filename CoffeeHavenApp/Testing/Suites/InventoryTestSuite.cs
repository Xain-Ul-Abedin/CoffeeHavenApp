using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CoffeeHavenApp.Testing.Base;
using CoffeeHavenApp.Testing.Mocks;
using CoffeeHavenDB.DAL;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;
using CoffeeHavenDB.Services;

namespace CoffeeHavenApp.Testing.Suites
{
    public class InventoryTestSuite : TestSuite
    {
        private IInventoryService _inventoryService;
        private InMemoryInventoryDAL _inventoryRepo;

        public InventoryTestSuite()
        {
            SuiteName = "Inventory Module (Admin Flows)";
        }

        public override void GlobalSetup()
        {
            _inventoryRepo = new InMemoryInventoryDAL();
            _inventoryService = new InventoryService(_inventoryRepo);
            
            _testCases.Clear();
            _testCases.Add(new StockThresholdTestCase(_inventoryService, _inventoryRepo));
            _testCases.Add(new RestockLogicTestCase(_inventoryService, _inventoryRepo));
        }

        public override void Setup()
        {
            _inventoryRepo.Reset();
        }

        private class StockThresholdTestCase : ITestCase
        {
            private IInventoryService _service;
            private InMemoryInventoryDAL _repo;
            public string Name => "Admin: Threshold Monitoring (Low Stock)";
            public string ErrorMessage { get; private set; }

            public StockThresholdTestCase(IInventoryService service, InMemoryInventoryDAL repo)
            {
                _service = service;
                _repo = repo;
            }

            public bool Run()
            {
                _repo.AddInventoryItem(101, 5);
                _repo.AddInventoryItem(102, 50);

                bool isLow = _service.CheckLowStock(101, 10);
                if (!isLow) { ErrorMessage = "Failed to detect low stock for Item 101."; return false; }

                bool isNotLow = _service.CheckLowStock(102, 10);
                if (isNotLow) { ErrorMessage = "Incorrectly flagged high-stock Item 102 as low."; return false; }

                DataTable lowItems = _service.GetLowStockItems(10);
                if (lowItems.Rows.Count != 1) { ErrorMessage = $"Expected 1 low stock item, found {lowItems.Rows.Count}."; return false; }

                return true;
            }
        }

        private class RestockLogicTestCase : ITestCase
        {
            private IInventoryService _service;
            private InMemoryInventoryDAL _repo;
            public string Name => "Admin: Programmatic Restocking";
            public string ErrorMessage { get; private set; }

            public RestockLogicTestCase(IInventoryService service, InMemoryInventoryDAL repo)
            {
                _service = service;
                _repo = repo;
            }

            public bool Run()
            {
                _repo.AddInventoryItem(101, 5);

                if (_service.CheckLowStock(101, 10) == false) { ErrorMessage = "Init check failed."; return false; }

                _service.RestockItem(101, 20);

                if (_service.CheckLowStock(101, 10) == true) { ErrorMessage = "Item 101 still flagged as low stock after restock."; return false; }

                return true;
            }
        }
    }
}
