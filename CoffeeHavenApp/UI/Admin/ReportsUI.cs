using System;
using System.Data;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenApp.UI.Base;

namespace CoffeeHavenApp.UI.Admin
{
    public static class ReportsUI
    {
        public static void ShowReportsMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("REPORTS & ANALYTICS");
                Console.WriteLine(" 1. Sales Summary (Revenue/Avg)");
                Console.WriteLine(" 2. Top Selling Products");
                Console.WriteLine(" 3. Inventory Health (Low Stock)");
                Console.WriteLine(" 4. Customer Activity (Top Spenders)");
                Console.WriteLine(" 5. Order Status Breakdown");
                Console.WriteLine(" 6. Back to Dashboard");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplaySalesSummary(context);
                        break;
                    case "2":
                        DisplayTopProducts(context);
                        break;
                    case "3":
                        DisplayInventoryHealth(context);
                        break;
                    case "4":
                        DisplayCustomerInsights(context);
                        break;
                    case "5":
                        DisplayOrderStatusAnalytics(context);
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

        private static void DisplaySalesSummary(UIContext context)
        {
            ConsoleHelper.DrawHeader("SALES SUMMARY");
            DataTable dt = context.ReportService.GetSalesSummary();

            if (dt.Rows.Count > 0 && dt.Rows[0]["TotalOrders"] != DBNull.Value)
            {
                var row = dt.Rows[0];
                Console.WriteLine($" Total Completed Orders : {row["TotalOrders"]}");
                Console.WriteLine($" Total Revenue          : ${Convert.ToDecimal(row["TotalRevenue"]):0.00}");
                Console.WriteLine($" Average Order Value   : ${Convert.ToDecimal(row["AverageOrderValue"]):0.00}");
            }
            else
            {
                ConsoleHelper.InfoMessage("No sales data available.");
            }
            ConsoleHelper.Pause();
        }

        private static void DisplayTopProducts(UIContext context)
        {
            ConsoleHelper.DrawHeader("TOP SELLING PRODUCTS");
            DataTable dt = context.ReportService.GetTopSellingProducts(5);

            if (dt.Rows.Count > 0)
            {
                Console.WriteLine("\n Name                 | Units Sold | Revenue");
                Console.WriteLine(" ----------------------------------------------");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($" {row["Name"],-20} | {row["TotalSold"],-10} | ${Convert.ToDecimal(row["Revenue"]):0.00}");
                }
            }
            else
            {
                ConsoleHelper.InfoMessage("No data available.");
            }
            ConsoleHelper.Pause();
        }

        private static void DisplayInventoryHealth(UIContext context)
        {
            ConsoleHelper.DrawHeader("INVENTORY HEALTH (LOW STOCK)");
            int threshold = 5;
            DataTable dt = context.ReportService.GetInventoryHealth(threshold);

            if (dt.Rows.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Items with stock <= {threshold}:");
                Console.ResetColor();
                Console.WriteLine("\n Name                 | Category   | Stock Remaining");
                Console.WriteLine(" ----------------------------------------------------");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($" {row["Name"],-20} | {row["Category"],-10} | {row["StockQuantity"]}");
                }
            }
            else
            {
                ConsoleHelper.SuccessMessage($"All active menu items are healthy (Stock > {threshold}).");
            }
            ConsoleHelper.Pause();
        }

        private static void DisplayCustomerInsights(UIContext context)
        {
            ConsoleHelper.DrawHeader("CUSTOMER ACTIVITY (TOP SPENDERS)");
            DataTable dt = context.ReportService.GetCustomerInsights(5);

            if (dt.Rows.Count > 0)
            {
                Console.WriteLine("\n Full Name            | Orders | Total Spend");
                Console.WriteLine(" --------------------------------------------");
                foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine($" {row["FullName"],-20} | {row["OrdersCount"],-6} | ${Convert.ToDecimal(row["TotalSpend"]):0.00}");
                }
            }
            else
            {
                ConsoleHelper.InfoMessage("No customer data available.");
            }
            ConsoleHelper.Pause();
        }

        private static void DisplayOrderStatusAnalytics(UIContext context)
        {
            ConsoleHelper.DrawHeader("ORDER STATUS BREAKDOWN");
            DataTable dt = context.ReportService.GetOrderStatusAnalytics();

            if (dt.Rows.Count > 0)
            {
                Console.WriteLine("\n Status      | Count | Total Volume");
                Console.WriteLine(" -----------------------------------");
                foreach (DataRow row in dt.Rows)
                {
                    decimal volume = row["Volume"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Volume"]);
                    Console.WriteLine($" {row["Status"],-11} | {row["Count"],-5} | ${volume:0.00}");
                }
            }
            else
            {
                ConsoleHelper.InfoMessage("No orders found.");
            }
            ConsoleHelper.Pause();
        }
    }
}
