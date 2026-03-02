using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB
{
    /// <summary>
    /// InventoryDAL handles stock-specific operations.
    /// Updated to implement IInventoryRepository for Lab 06.
    /// </summary>
    public class InventoryDAL : IInventoryRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

        public DataTable GetLowStockItems(int threshold = 10)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, Name, StockQuantity FROM MenuItems WHERE StockQuantity <= @threshold AND IsActive = 1";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@threshold", threshold);
                try { da.Fill(dt); }
                catch (SqlException ex) { Console.WriteLine("\n[ERROR] " + ex.Message); }
            }
            return dt;
        }

        public void RestockItem(int itemId, int quantityToAdd)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE MenuItems SET StockQuantity = StockQuantity + @qty WHERE ItemID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", itemId);
                cmd.Parameters.AddWithValue("@qty", quantityToAdd);
                try
                {
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0) Console.WriteLine($"\n[SUCCESS] Item {itemId} restocked.");
                    else Console.WriteLine("\n[NOT FOUND] Item ID not found.");
                }
                catch (SqlException ex) { Console.WriteLine("\n[ERROR] " + ex.Message); }
            }
        }
    }
}