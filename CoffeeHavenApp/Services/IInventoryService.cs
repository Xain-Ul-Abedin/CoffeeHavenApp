using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    /// <summary>
    /// Updated for Lab 07: Contract for Inventory Business Logic.
    /// Added threshold monitoring to support programmatic low-stock alerts.
    /// </summary>
    public interface IInventoryService
    {
        DataTable GetLowStockItems(int threshold);
        void RestockItem(int itemId, int quantityToAdd);

        // New BLL Method for Lab 07
        bool CheckLowStock(int productId, int threshold);
    }
}