using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenDB.Models;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenApp.Helpers;

namespace CoffeeHavenApp.UI.Admin
{
    public static class ProductManagementUI
    {
        public static void ShowProductManagementMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("PRODUCT MANAGEMENT");
                Console.WriteLine(" 1. View Products");
                Console.WriteLine(" 2. Search Products");
                Console.WriteLine(" 3. Add Product");
                Console.WriteLine(" 4. Update Product");
                Console.WriteLine(" 5. Delete Product");
                Console.WriteLine(" 6. Back");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplayProductTable(context.ProductService.GetAllProducts());
                        ConsoleHelper.Pause();
                        break;
                    case "2":
                        SearchProductsFlow(context);
                        break;
                    case "3":
                        AddProductFlow(context);
                        break;
                    case "4":
                        UpdateProductFlow(context);
                        break;
                    case "5":
                        DeleteProductFlow(context);
                        break;
                    case "6":
                        return;
                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        public static void DisplayProductTable(List<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("No products available.");
                return;
            }

            Console.WriteLine("\n ID | Name                 | Price      | Stock");
            Console.WriteLine("--------------------------------------------------------");

            foreach (Product p in products.OrderBy(x => x.ProductId))
            {
                Console.WriteLine($" {p.ProductId,-2} | {p.ProductName,-20} | Rs. {p.Price,8:0.00} | {p.StockQuantity}");
            }
        }

        private static void SearchProductsFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("SEARCH PRODUCTS");
            Console.WriteLine(" 1. By Name / Description");
            Console.WriteLine(" 2. By Product ID");
            Console.WriteLine(" 3. By Price Range");
            Console.WriteLine(" 4. Back");

            string choice = ConsoleHelper.Prompt("Selection");
            List<Product> results = new List<Product>();

            switch (choice)
            {
                case "1":
                    string kw = ConsoleHelper.Prompt("Enter keyword");
                    results = SearchHelper.SearchProducts(context.ProductService.GetAllProducts(), kw);
                    break;
                case "2":
                    int id = ConsoleHelper.SafeReadInt("Enter Product ID");
                    results = SearchHelper.SearchProductsById(context.ProductService.GetAllProducts(), id);
                    break;
                case "3":
                    decimal min = ConsoleHelper.SafeReadDecimal("Min Price");
                    decimal max = ConsoleHelper.SafeReadDecimal("Max Price");
                    results = SearchHelper.FilterByPriceRange(context.ProductService.GetAllProducts(), min, max);
                    break;
                case "4": return;
            }

            if (results.Count == 0)
            {
                ConsoleHelper.InfoMessage("No products matched your criteria.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.SuccessMessage($"{results.Count} product(s) found:");
            Console.WriteLine("\nSort by: 1. Name  2. Price  3. Stock  (Enter to skip)");
            string sortChoice = ConsoleHelper.Prompt("Sort option");
            switch (sortChoice)
            {
                case "1": results = SearchHelper.SortProducts(results, "name"); break;
                case "2": results = SearchHelper.SortProducts(results, "price"); break;
                case "3": results = SearchHelper.SortProducts(results, "stock"); break;
            }

            DisplayProductTable(results);
            ConsoleHelper.Pause();
        }

        private static void AddProductFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("ADD PRODUCT");
            Product p = new Product
            {
                ProductName = ConsoleHelper.Prompt("Product Name"),
                Description = ConsoleHelper.Prompt("Description"),
                Price = ConsoleHelper.SafeReadDecimal("Price"),
                StockQuantity = ConsoleHelper.SafeReadInt("Initial Stock"),
                DiscountPercentage = ConsoleHelper.SafeReadDecimal("Discount Percentage"),
                CategoryId = ConsoleHelper.SafeReadInt("Category ID")
            };
            context.ProductService.AddProduct(p);
            ConsoleHelper.Pause();
        }

        private static void UpdateProductFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("UPDATE PRODUCT");
            DisplayProductTable(context.ProductService.GetAllProducts());

            int id = ConsoleHelper.SafeReadInt("\nEnter Product ID to update");
            Product existing = context.ProductService.GetProductById(id);
            if (existing == null) { ConsoleHelper.ErrorMessage("Product not found."); ConsoleHelper.Pause(); return; }

            existing.ProductName = ConsoleHelper.PromptDefault("Product Name", existing.ProductName);
            existing.Description = ConsoleHelper.PromptDefault("Description", existing.Description);
            existing.Price = ConsoleHelper.SafeReadDecimalDefault("Price", existing.Price);
            existing.StockQuantity = ConsoleHelper.SafeReadIntDefault("Stock Quantity", existing.StockQuantity);
            existing.DiscountPercentage = ConsoleHelper.SafeReadDecimalDefault("Discount Percentage", existing.DiscountPercentage);
            existing.CategoryId = ConsoleHelper.SafeReadIntDefault("Category ID", existing.CategoryId);

            context.ProductService.UpdateProduct(id, existing);
            ConsoleHelper.Pause();
        }

        private static void DeleteProductFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("DELETE PRODUCT");
            DisplayProductTable(context.ProductService.GetAllProducts());
            int id = ConsoleHelper.SafeReadInt("\nEnter Product ID to delete");
            if (ConsoleHelper.Confirm($"Delete product #{id}? Type YES to confirm: "))
            {
                context.ProductService.DeleteProduct(id);
            }
            else
            {
                ConsoleHelper.InfoMessage("Deletion cancelled.");
            }
            ConsoleHelper.Pause();
        }
    }
}
