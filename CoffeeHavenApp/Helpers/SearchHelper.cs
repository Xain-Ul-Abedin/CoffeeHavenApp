using CoffeeHavenDB.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CoffeeHavenApp.Helpers
{
    /// <summary>
    /// Reusable search and filter utilities for Coffee Haven.
    /// Works with Product lists, cart lists, and DataTable-based order/inventory views.
    /// </summary>
    public static class SearchHelper
    {
        // ============================================================
        // PRODUCT SEARCH & FILTER
        // ============================================================

        /// <summary>
        /// Searches products by name or description using a case-insensitive
        /// keyword match. Returns all products whose ProductName OR Description
        /// contains the search term.
        /// </summary>
        public static List<Product> SearchProducts(List<Product> products, string searchTerm)
        {
            if (products == null || products.Count == 0) return new List<Product>();
            if (string.IsNullOrWhiteSpace(searchTerm)) return products;

            string term = searchTerm.Trim().ToLowerInvariant();
            return products
                .Where(p => (p.ProductName?.ToLowerInvariant().Contains(term) ?? false) || 
                            (p.Description?.ToLowerInvariant().Contains(term) ?? false))
                .ToList();
        }

        public static List<Product> SearchProductsById(List<Product> products, int id)
        {
            if (products == null) return new List<Product>();
            return products.Where(p => p.ProductId == id).ToList();
        }

        /// <summary>
        /// Filters products by a price range (inclusive on both ends).
        /// Pass 0 for minPrice to skip the lower-bound check.
        /// Pass decimal.MaxValue for maxPrice to skip the upper-bound check.
        /// </summary>
        public static List<Product> FilterByPriceRange(List<Product> products, decimal minPrice, decimal maxPrice)
        {
            if (products == null || products.Count == 0)
                return new List<Product>();

            return products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToList();
        }

        /// <summary>
        /// Filters products by CategoryId.
        /// </summary>
        public static List<Product> FilterByCategory(List<Product> products, int categoryId)
        {
            if (products == null || products.Count == 0)
                return new List<Product>();

            return products
                .Where(p => p.CategoryId == categoryId)
                .ToList();
        }

        /// <summary>
        /// Returns only products that are currently in stock (StockQuantity > 0).
        /// </summary>
        public static List<Product> FilterInStock(List<Product> products)
        {
            if (products == null || products.Count == 0)
                return new List<Product>();

            return products
                .Where(p => p.StockQuantity > 0)
                .ToList();
        }

        /// <summary>
        /// Sorts a product list by the specified column. Supports:
        /// "name", "price", "stock", "category", "discount".
        /// </summary>
        public static List<Product> SortProducts(List<Product> products, string sortBy, bool ascending = true)
        {
            if (products == null || products.Count == 0)
                return new List<Product>();

            string key = (sortBy ?? "name").Trim().ToLowerInvariant();

            IEnumerable<Product> sorted;

            switch (key)
            {
                case "price":
                    sorted = ascending
                        ? products.OrderBy(p => p.Price)
                        : products.OrderByDescending(p => p.Price);
                    break;

                case "stock":
                    sorted = ascending
                        ? products.OrderBy(p => p.StockQuantity)
                        : products.OrderByDescending(p => p.StockQuantity);
                    break;

                case "category":
                    sorted = ascending
                        ? products.OrderBy(p => p.CategoryId)
                        : products.OrderByDescending(p => p.CategoryId);
                    break;

                case "discount":
                    sorted = ascending
                        ? products.OrderBy(p => p.DiscountPercentage)
                        : products.OrderByDescending(p => p.DiscountPercentage);
                    break;

                case "name":
                default:
                    sorted = ascending
                        ? products.OrderBy(p => p.ProductName)
                        : products.OrderByDescending(p => p.ProductName);
                    break;
            }

            return sorted.ToList();
        }

        // ============================================================
        // CART SEARCH
        // ============================================================

        /// <summary>
        /// Searches the shopping cart items by product name.
        /// Returns only matching items from the cart.
        /// </summary>
        public static List<Product> SearchCart(List<Product> cartItems, string searchTerm)
        {
            if (cartItems == null || cartItems.Count == 0)
                return new List<Product>();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return cartItems;

            string term = searchTerm.Trim().ToLowerInvariant();

            return cartItems
                .Where(p => !string.IsNullOrEmpty(p.ProductName) &&
                            p.ProductName.ToLowerInvariant().Contains(term))
                .ToList();
        }

        // ============================================================
        // DATATABLE SEARCH (Orders / Inventory)
        // ============================================================

        /// <summary>
        /// Searches a DataTable by looking for the search term in every string-type
        /// column. Returns a filtered DataTable with only matching rows.
        /// Useful for order-history and inventory DataTables.
        /// </summary>
        public static DataTable SearchDataTable(DataTable table, string searchTerm)
        {
            if (table == null || table.Rows.Count == 0)
                return table ?? new DataTable();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return table;

            string term = searchTerm.Trim().ToLowerInvariant();

            DataTable result = table.Clone(); // copy schema

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    string cellValue = row[col]?.ToString();
                    if (!string.IsNullOrEmpty(cellValue) &&
                        cellValue.ToLowerInvariant().Contains(term))
                    {
                        result.ImportRow(row);
                        break; // row already matched — move on
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Filters a DataTable by a specific column name and value.
        /// Performs a case-insensitive string comparison.
        /// </summary>
        public static DataTable FilterDataTableByColumn(DataTable table, string columnName, string value)
        {
            if (table == null || table.Rows.Count == 0)
                return table ?? new DataTable();

            if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(value))
                return table;

            if (!table.Columns.Contains(columnName))
                return table;

            string target = value.Trim().ToLowerInvariant();

            DataTable result = table.Clone();

            foreach (DataRow row in table.Rows)
            {
                string cellValue = row[columnName]?.ToString();
                if (!string.IsNullOrEmpty(cellValue) &&
                    cellValue.ToLowerInvariant().Contains(target))
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }

        public static DataTable FilterByStatus(DataTable table, string status)
        {
             return FilterDataTableByColumn(table, "Status", status);
        }
    }
}