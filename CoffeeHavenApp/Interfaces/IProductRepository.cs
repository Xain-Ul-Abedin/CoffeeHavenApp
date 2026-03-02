// Part of Lab 6: Defining the Contract for Data Access
using CoffeeHavenDB.Models;

public interface IProductRepository
{
    void AddProduct(Product product);
    void UpdateProduct(int productId, Product updatedProduct);
    void DeleteProduct(int productId);
    List<Product> GetAllProducts();
    Product GetProductById(int productId);
}