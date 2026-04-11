using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenApp.Testing.Mocks
{
    public class InMemoryInventoryDAL : IInventoryRepository
    {
        private List<InventoryMock> _inventory = new List<InventoryMock>();

        public void Reset()
        {
            _inventory.Clear();
        }

        // Internal method for setup
        public void AddInventoryItem(int itemId, int initialStock)
        {
            _inventory.Add(new InventoryMock { ItemId = itemId, Stock = initialStock });
        }

        public DataTable GetLowStockItems(int threshold)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemID", typeof(int));
            dt.Columns.Add("StockQuantity", typeof(int));

            foreach (var item in _inventory.Where(i => i.Stock < threshold))
            {
                dt.Rows.Add(item.ItemId, item.Stock);
            }
            return dt;
        }

        public void RestockItem(int itemId, int quantityToAdd)
        {
            var item = _inventory.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                item.Stock += quantityToAdd;
            }
        }

        private class InventoryMock
        {
            public int ItemId { get; set; }
            public int Stock { get; set; }
        }
    }
}
