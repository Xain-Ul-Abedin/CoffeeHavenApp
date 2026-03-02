using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB
{
    /// <summary>
    /// OrderDAL handles the transactional logic of placing and managing orders.
    /// Updated to implement IOrderRepository for Lab 06.
    /// </summary>
    public class OrderDAL : IOrderRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

        // ============================================================
        // PLACE ORDER (Single Item Overload)
        // ============================================================
        public void PlaceOrder(int userId, int itemId, int quantity)
        {
            var cart = new Dictionary<int, int> { { itemId, quantity } };
            PlaceOrder(userId, cart);
        }

        // ============================================================
        // PLACE ORDER (Multi-Item Shopping Cart Transaction)
        // ============================================================
        public void PlaceOrder(int userId, Dictionary<int, int> itemsToOrder)
        {
            if (itemsToOrder == null || itemsToOrder.Count == 0)
                throw new Exception("Shopping cart is empty.");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // 1. Pre-Check User
                string userCheck = "SELECT COUNT(*) FROM Users WHERE UserID = @uId";
                using (SqlCommand cmdUser = new SqlCommand(userCheck, con))
                {
                    cmdUser.Parameters.AddWithValue("@uId", userId);
                    if ((int)cmdUser.ExecuteScalar() == 0) throw new Exception("Invalid User Session.");
                }

                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    decimal grandTotal = 0;
                    var snapshots = new List<OrderItemSnapshot>();

                    // 2. Validate Inventory & Prices
                    foreach (var entry in itemsToOrder)
                    {
                        int itemId = entry.Key;
                        int qty = entry.Value;

                        string checkQuery = "SELECT Name, BasePrice, StockQuantity FROM MenuItems WHERE ItemID = @iId AND IsActive = 1";
                        SqlCommand cmdCheck = new SqlCommand(checkQuery, con, trans);
                        cmdCheck.Parameters.AddWithValue("@iId", itemId);

                        using (SqlDataReader reader = cmdCheck.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int stock = (int)reader["StockQuantity"];
                                decimal price = (decimal)reader["BasePrice"];
                                string name = reader["Name"].ToString();

                                if (stock < qty) throw new Exception($"Insufficient stock for {name}. Available: {stock}");

                                decimal sub = price * qty;
                                grandTotal += sub;
                                snapshots.Add(new OrderItemSnapshot { ItemId = itemId, UnitPrice = price, Quantity = qty, Subtotal = sub });
                            }
                            else throw new Exception($"Product ID {itemId} is no longer available on the menu.");
                        }
                    }

                    // 3. Insert Parent Order
                    string orderQuery = @"INSERT INTO Orders (UserID, OrderDate, TotalAmount) 
                                       OUTPUT INSERTED.OrderID 
                                       VALUES (@uId, GETDATE(), @total)";
                    SqlCommand cmdOrder = new SqlCommand(orderQuery, con, trans);
                    cmdOrder.Parameters.AddWithValue("@uId", userId);
                    cmdOrder.Parameters.AddWithValue("@total", grandTotal);

                    int newOrderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    // 4. Insert OrderItems & Update Stock
                    foreach (var item in snapshots)
                    {
                        string itemQuery = @"INSERT INTO OrderItems (OrderID, ItemID, Quantity, UnitPrice, ItemSubtotal) 
                                           VALUES (@oId, @iId, @qty, @price, @sub)";
                        SqlCommand cmdItem = new SqlCommand(itemQuery, con, trans);
                        cmdItem.Parameters.AddWithValue("@oId", newOrderId);
                        cmdItem.Parameters.AddWithValue("@iId", item.ItemId);
                        cmdItem.Parameters.AddWithValue("@qty", item.Quantity);
                        cmdItem.Parameters.AddWithValue("@price", item.UnitPrice);
                        cmdItem.Parameters.AddWithValue("@sub", item.Subtotal);
                        cmdItem.ExecuteNonQuery();

                        string stockQuery = "UPDATE MenuItems SET StockQuantity = StockQuantity - @qty WHERE ItemID = @iId";
                        SqlCommand cmdStock = new SqlCommand(stockQuery, con, trans);
                        cmdStock.Parameters.AddWithValue("@qty", item.Quantity);
                        cmdStock.Parameters.AddWithValue("@iId", item.ItemId);
                        cmdStock.ExecuteNonQuery();
                    }

                    trans.Commit();
                    Console.WriteLine($"\n[SUCCESS] Order #{newOrderId} processed. Grand Total: ${grandTotal:0.00}");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        // ============================================================
        // CANCEL ORDER: Restores Stock & Deletes Records (FK Safe)
        // ============================================================
        public void CancelOrder(int orderId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();
                try
                {
                    // 1. Get Items for Restocking
                    string query = "SELECT ItemID, Quantity FROM OrderItems WHERE OrderID = @oId";
                    SqlCommand cmd = new SqlCommand(query, con, trans);
                    cmd.Parameters.AddWithValue("@oId", orderId);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) { da.Fill(dt); }

                    if (dt.Rows.Count == 0) throw new Exception("Order not found.");

                    // 2. Restore Stock
                    foreach (DataRow row in dt.Rows)
                    {
                        string restore = "UPDATE MenuItems SET StockQuantity = StockQuantity + @qty WHERE ItemID = @iId";
                        SqlCommand cmdRes = new SqlCommand(restore, con, trans);
                        cmdRes.Parameters.AddWithValue("@qty", (int)row["Quantity"]);
                        cmdRes.Parameters.AddWithValue("@iId", (int)row["ItemID"]);
                        cmdRes.ExecuteNonQuery();
                    }

                    // 3. Delete Records (OrderItems first, then Orders)
                    new SqlCommand($"DELETE FROM OrderItems WHERE OrderID = {orderId}", con, trans).ExecuteNonQuery();
                    int rows = new SqlCommand($"DELETE FROM Orders WHERE OrderID = {orderId}", con, trans).ExecuteNonQuery();

                    if (rows > 0)
                    {
                        trans.Commit();
                        Console.WriteLine($"\n[SUCCESS] Order #{orderId} cancelled. Inventory has been returned to stock.");
                    }
                }
                catch (Exception ex) { trans.Rollback(); throw ex; }
            }
        }

        public DataTable GetUserOrderHistory(int userId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT o.OrderID, o.OrderDate, m.Name as ProductName, 
                                 oi.Quantity, oi.UnitPrice, oi.ItemSubtotal as SubTotal
                                 FROM Orders o 
                                 JOIN OrderItems oi ON o.OrderID = oi.OrderID 
                                 JOIN MenuItems m ON oi.ItemID = m.ItemID 
                                 WHERE o.UserID = @uId ORDER BY o.OrderDate DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@uId", userId);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }

        private class OrderItemSnapshot
        {
            public int ItemId { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}