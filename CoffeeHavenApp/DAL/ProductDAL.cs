using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Models;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB.DAL
{
    // Part of Lab 6: Refactored to implement IProductRepository
    // This class now handles the SQL implementation of the interface
    public class ProductDAL : IProductRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

        public void AddProduct(Product product)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO MenuItems (Name, Description, BasePrice, CategoryID, IsActive, StockQuantity) 
                               VALUES (@name, @desc, @price, @catId, 1, @stock)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", product.ProductName);
                cmd.Parameters.AddWithValue("@desc", (object)product.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@price", product.Price);
                cmd.Parameters.AddWithValue("@catId", product.CategoryId == 0 ? 1 : product.CategoryId);
                cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
                con.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("\n[DATABASE] Product Added Successfully!");
            }
        }

        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Description, BasePrice, StockQuantity FROM MenuItems WHERE IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductId = (int)reader["ItemID"],
                            ProductName = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = (decimal)reader["BasePrice"],
                            StockQuantity = (int)reader["StockQuantity"]
                        });
                    }
                }
            }
            return products;
        }

        public void UpdateProduct(int productId, Product updatedProduct)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE MenuItems 
                               SET Name = @name, Description = @desc, BasePrice = @price, StockQuantity = @stock 
                               WHERE ItemID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", productId);
                cmd.Parameters.AddWithValue("@name", updatedProduct.ProductName);
                cmd.Parameters.AddWithValue("@desc", (object)updatedProduct.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@price", updatedProduct.Price);
                cmd.Parameters.AddWithValue("@stock", updatedProduct.StockQuantity);
                con.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) Console.WriteLine("\n[DATABASE] Product Updated Successfully!");
                else Console.WriteLine("\n[DATABASE] Product Not Found.");
            }
        }

        public void DeleteProduct(int productId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE MenuItems SET IsActive = 0 WHERE ItemID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", productId);
                con.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) Console.WriteLine("\n[DATABASE] Product Deleted Successfully!");
                else Console.WriteLine("\n[DATABASE] Product Not Found.");
            }
        }

        public Product GetProductById(int productId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, Description, BasePrice, StockQuantity FROM MenuItems WHERE ItemID = @id AND IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", productId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Product
                        {
                            ProductId = (int)reader["ItemID"],
                            ProductName = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = (decimal)reader["BasePrice"],
                            StockQuantity = (int)reader["StockQuantity"]
                        };
                    }
                }
            }
            return null;
        }
    }
}