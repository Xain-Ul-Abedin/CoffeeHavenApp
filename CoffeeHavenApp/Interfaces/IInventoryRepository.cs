using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    // Part of Lab 6: Contract for Inventory Data Access
    public interface IInventoryRepository
    {
        DataTable GetLowStockItems(int threshold);
        void RestockItem(int itemId, int quantityToAdd);
    }
}