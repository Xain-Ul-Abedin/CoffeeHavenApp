using System;
using System.Collections.Generic;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;


namespace CoffeeHavenApp.Services
{
    /// <summary>
    /// Lab 07 Implementation: ProductService BLL
    /// Handles business validation, automated discounting, and inventory guardrails.
    /// This implementation is synchronized with the existing 'StockQuantity' naming convention.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Constructor accepting an IProductRepository dependency (Injection)
        /// </summary>
        /// <param name="repository">The data access implementation (SQL or InMemory)</param>
        public ProductService(IProductRepository repository)
        {
            _productRepository = repository;
        }

        // ============================================================
        // CORE CRUD OPERATIONS WITH BUSINESS LOGIC
        // ============================================================

        /// <summary>
        /// Adds a product after performing business validation and applying discounts.
        /// </summary>
        public void AddProduct(Product product)
        {
            Console.WriteLine($"[BLL] Validating new product: {product.ProductName}");

            if (IsValidProduct(product))
            {
                // Business Rule: Automated Discount Calculation
                ApplyDiscount(product);

                _productRepository.AddProduct(product);
                Console.WriteLine("[BLL] Product passed validation and has been sent to the Data Access Layer.");
            }
            else
            {
                Console.WriteLine("[BLL] ERROR: Invalid Product Data! Ensure Name is provided, Price > 0, and Stock >= 0.");
            }
        }

        /// <summary>
        /// Updates an existing product with business logic checks.
        /// </summary>
        public void UpdateProduct(int productId, Product updatedProduct)
        {
            if (IsValidProduct(updatedProduct))
            {
                ApplyDiscount(updatedProduct);
                _productRepository.UpdateProduct(productId, updatedProduct);
                Console.WriteLine($"[BLL] Product ID {productId} updated with business rules applied.");
            }
            else
            {
                Console.WriteLine($"[BLL] ERROR: Update for Product ID {productId} failed due to invalid data.");
            }
        }

        /// <summary>
        /// Deletes a product after verifying it exists in the system.
        /// </summary>
        public void DeleteProduct(int productId)
        {
            var product = _productRepository.GetProductById(productId);
            if (product != null)
            {
                _productRepository.DeleteProduct(productId);
                Console.WriteLine($"[BLL] Product '{product.ProductName}' has been successfully processed for deletion.");
            }
            else
            {
                Console.WriteLine("[BLL] ERROR: Product not found for deletion.");
            }
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product GetProductById(int productId)
        {
            return _productRepository.GetProductById(productId);
        }

        // ============================================================
        // PRIVATE BUSINESS LOGIC METHODS
        // ============================================================

        /// <summary>
        /// Business Logic Rule: Recalculates price if a discount is present.
        /// </summary>
        private void ApplyDiscount(Product product)
        {
            if (product.DiscountPercentage > 0)
            {
                decimal discountAmount = (product.DiscountPercentage / 100) * product.Price;
                product.Price -= discountAmount;
                Console.WriteLine($"[BLL] Logic: {product.DiscountPercentage}% discount applied. Final Price: ${product.Price:F2}");
            }
        }

        /// <summary>
        /// Business Logic Rule: Validates the state of a product object.
        /// Matches the 'StockQuantity' property in your Product Model.
        /// </summary>
        private bool IsValidProduct(Product product)
        {
            if (string.IsNullOrEmpty(product.ProductName) || product.Price <= 0 || product.StockQuantity < 0)
            {
                return false;
            }
            return true;
        }

        // ============================================================
        // INVENTORY MANAGEMENT LOGIC (Lab 07 Orchestration)
        // ============================================================

        /// <summary>
        /// Reduces stock levels after a successful sale verification.
        /// </summary>
        public void ReduceStock(int productId, int quantity)
        {
            var product = _productRepository.GetProductById(productId);
            if (product != null)
            {
                if (product.StockQuantity >= quantity)
                {
                    product.StockQuantity -= quantity;
                    // Persist the stock change back to the repository
                    _productRepository.UpdateProduct(productId, product);
                    Console.WriteLine($"[BLL] Inventory updated for {product.ProductName}. Remaining Stock: {product.StockQuantity}");
                }
                else
                {
                    Console.WriteLine($"[BLL] ERROR: Insufficient stock for {product.ProductName}!");
                }
            }
            else
            {
                Console.WriteLine("[BLL] ERROR: Product not found for stock reduction.");
            }
        }

        /// <summary>
        /// Checks if a product is available in the required quantity.
        /// Used by the OrderService to prevent over-selling.
        /// </summary>
        public bool IsProductAvailable(int productId, int quantity)
        {
            var product = _productRepository.GetProductById(productId);
            return product != null && product.StockQuantity >= quantity;
        }
    }
}