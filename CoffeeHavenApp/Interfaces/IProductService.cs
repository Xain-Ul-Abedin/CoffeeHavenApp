using System.Collections.Generic;
using CoffeeHavenDB.Models;

namespace CoffeeHavenDB.Interfaces
{
    /// <summary>
    /// Lab 07 Update: Added methods for Business Logic orchestration.
    /// These allow the OrderService to verify and deduct stock through the BLL.
    /// </summary>
    public interface IProductService
    {
        void AddProduct(Product product);
        void UpdateProduct(int productId, Product updatedProduct);
        void DeleteProduct(int productId);
        List<Product> GetAllProducts();
        Product GetProductById(int productId);

        // Required for Lab 07
        bool IsProductAvailable(int productId, int quantity);
        void ReduceStock(int productId, int quantity);
    }
}