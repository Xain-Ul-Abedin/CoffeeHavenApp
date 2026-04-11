using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB
{
    public class ReportDAL : IReportRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

        public DataTable GetSalesSummary()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT 
                                 COUNT(OrderID) as TotalOrders, 
                                 SUM(TotalAmount) as TotalRevenue, 
                                 AVG(TotalAmount) as AverageOrderValue 
                                 FROM Orders 
                                 WHERE Status = 'Completed'";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }

        public DataTable GetTopSellingProducts(int topN)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = $@"SELECT TOP {topN} 
                                 m.Name, 
                                 SUM(oi.Quantity) as TotalSold, 
                                 SUM(oi.ItemSubtotal) as Revenue 
                                 FROM OrderItems oi 
                                 JOIN MenuItems m ON oi.ItemID = m.ItemID 
                                 JOIN Orders o ON oi.OrderID = o.OrderID
                                 WHERE o.Status = 'Completed'
                                 GROUP BY m.Name 
                                 ORDER BY TotalSold DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }

        public DataTable GetInventoryHealth(int lowStockThreshold)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = $@"SELECT Name, StockQuantity, Category 
                                 FROM MenuItems 
                                 WHERE StockQuantity <= {lowStockThreshold} AND IsActive = 1
                                 ORDER BY StockQuantity ASC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }

        public DataTable GetCustomerInsights(int topN)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = $@"SELECT TOP {topN} 
                                 u.FullName, 
                                 u.Email, 
                                 COUNT(o.OrderID) as OrdersCount, 
                                 SUM(o.TotalAmount) as TotalSpend 
                                 FROM Users u 
                                 JOIN Orders o ON u.UserID = o.UserID 
                                 WHERE o.Status = 'Completed'
                                 GROUP BY u.FullName, u.Email 
                                 ORDER BY TotalSpend DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }

        public DataTable GetOrderStatusAnalytics()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT Status, COUNT(OrderID) as Count, SUM(TotalAmount) as Volume 
                                 FROM Orders 
                                 GROUP BY Status 
                                 ORDER BY Count DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                try { da.Fill(dt); } catch { }
            }
            return dt;
        }
    }
}
