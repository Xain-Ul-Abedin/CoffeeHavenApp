using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeHavenDB.Models;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB.DAL
{
    // Lab 06: This implements the same interface but uses a LIST instead of SQL.
    // Enhanced with verification logic to ensure data integrity.
    public class InMemoryProductDAL : IProductRepository
    {
        private List<Product> _products = new List<Product>();

        public void AddProduct(Product product)
        {
            // VERIFICATION: Ensure the object is not null and has required data
            if (product == null)
            {
                Console.WriteLine("\n[ERROR] Attempted to add a null product object.");
                return;
            }

            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                Console.WriteLine("\n[ERROR] Product Name is mandatory.");
                return;
            }

            if (product.Price < 0)
            {
                Console.WriteLine("\n[ERROR] Product Price cannot be negative.");
                return;
            }

            // Generate a unique ID based on the current max
            product.ProductId = _products.Count > 0 ? _products.Max(p => p.ProductId) + 1 : 1;

            _products.Add(product);
            Console.WriteLine($"\n[MEMORY] Product '{product.ProductName}' (ID: {product.ProductId}) Added Successfully!");
        }

        public List<Product> GetAllProducts() => _products;

        public void UpdateProduct(int productId, Product updatedProduct)
        {
            if (updatedProduct == null)
            {
                Console.WriteLine("\n[ERROR] Update data is missing.");
                return;
            }

            // VERIFICATION: Check if the item exists before attempting an update
            var existingProduct = _products.FirstOrDefault(p => p.ProductId == productId);
            if (existingProduct != null)
            {
                // Apply changes
                existingProduct.ProductName = updatedProduct.ProductName;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.Description = updatedProduct.Description;
                existingProduct.StockQuantity = updatedProduct.StockQuantity;

                Console.WriteLine($"\n[MEMORY] Product ID {productId} Updated Successfully!");
            }
            else
            {
                Console.WriteLine($"\n[MEMORY] Update Failed: Product ID {productId} does not exist in records.");
            }
        }

        public void DeleteProduct(int productId)
        {
            // VERIFICATION: Ensure the product exists before removal
            var product = _products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                _products.Remove(product);
                Console.WriteLine($"\n[MEMORY] Product ID {productId} Deleted Successfully!");
            }
            else
            {
                Console.WriteLine($"\n[MEMORY] Delete Failed: Product ID {productId} not found.");
            }
        }

        public Product GetProductById(int productId)
        {
            var product = _products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                Console.WriteLine($"\n[MEMORY] Warning: Product ID {productId} not found.");
            }
            return product;
        }
    }
}