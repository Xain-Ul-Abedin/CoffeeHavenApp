using System;
using System.Data;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB.Services
{
    /// <summary>
    /// Lab 07 Implementation: InventoryService BLL
    /// Handles restocking validation and low-stock threshold monitoring.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        /// <summary>
        /// Restocks an item while enforcing a positive quantity guardrail.
        /// </summary>
        public void RestockItem(int itemId, int quantityToAdd)
        {
            Console.WriteLine($"[BLL] Processing restock request for Item ID: {itemId}");

            // 1. [LOGIC] Restock Guardrail: Quantity must be greater than zero
            if (quantityToAdd <= 0)
            {
                Console.WriteLine($"[BLL] ERROR: Invalid restock quantity ({quantityToAdd}). You can only add positive amounts.");
                return;
            }

            // 2. [DAL] Update the database via the repository
            _inventoryRepository.RestockItem(itemId, quantityToAdd);
            Console.WriteLine($"[BLL] Success: {quantityToAdd} units added to stock.");
        }

        public DataTable GetLowStockItems(int threshold)
        {
            return _inventoryRepository.GetLowStockItems(threshold);
        }

        /// <summary>
        /// Business Logic: Programmatic monitoring of stock thresholds.
        /// Returns true if the specific product is below the warning level.
        /// </summary>
        public bool CheckLowStock(int productId, int threshold)
        {
            DataTable lowStockItems = _inventoryRepository.GetLowStockItems(threshold);

            // Iterate through the low-stock data to see if our product matches
            foreach (DataRow row in lowStockItems.Rows)
            {
                // Note: Using 'ItemID' to match your InventoryDAL's SELECT columns
                if (Convert.ToInt32(row["ItemID"]) == productId)
                {
                    Console.WriteLine($"[BLL] WARNING: Item ID {productId} is currently below the {threshold} unit threshold!");
                    return true;
                }
            }

            return false;
        }
    }
}