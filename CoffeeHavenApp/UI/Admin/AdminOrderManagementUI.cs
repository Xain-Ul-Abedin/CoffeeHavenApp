using System;
using System.Data;
using System.Linq;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenApp.Helpers;

namespace CoffeeHavenApp.UI.Admin
{
    public static class AdminOrderManagementUI
    {
        public static void ShowAdminOrderMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("ADMIN: ALL ORDERS MANAGEMENT");
                DataTable allOrders = context.OrderService.GetAllOrders();

                Console.WriteLine("\n Order# | Customer             | Date       | Status    | Method      | Total");
                Console.WriteLine(" ----------------------------------------------------------------------------------");
                if (allOrders == null || allOrders.Rows.Count == 0)
                {
                    Console.WriteLine(" No orders found in the system.");
                }
                else
                {
                    foreach (DataRow row in allOrders.Rows)
                    {
                        Console.WriteLine($" {row["OrderID"],-6} | {row["Customer"],-20} | {Convert.ToDateTime(row["OrderDate"]):MM/dd/yy} | {row["Status"],-9} | {row["PaymentMethod"],-11} | Rs. {Convert.ToDecimal(row["GrantTotal"]):0.00}");
                    }
                }

                Console.WriteLine("\n 1. Update Order Status  2. Search All Orders  3. Filter by Status  4. Back");
                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        UpdateAnyOrderStatusFlow(context);
                        break;
                    case "2":
                        SearchAllOrdersFlow(context, allOrders);
                        break;
                    case "3":
                        FilterAllOrdersByStatusFlow(context, allOrders);
                        break;
                    case "4":
                        return;
                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private static void UpdateAnyOrderStatusFlow(UIContext context)
        {
            int id = ConsoleHelper.SafeReadInt("\nEnter Order ID to override (0 to cancel)");
            if (id <= 0) return;

            Console.WriteLine("\nSet Status to:");
            Console.WriteLine(" 1. Pending");
            Console.WriteLine(" 2. Completed");
            Console.WriteLine(" 3. Cancelled");
            string choice = ConsoleHelper.Prompt("Selection");

            string newStatus = "";
            if (choice == "1") newStatus = "Pending";
            else if (choice == "2") newStatus = "Completed";
            else if (choice == "3") newStatus = "Cancelled";
            else return;

            try
            {
                context.OrderService.UpdateOrderStatus(id, newStatus);
                ConsoleHelper.SuccessMessage($"Order #{id} status overridden to '{newStatus}'.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorMessage(ex.Message);
            }
            ConsoleHelper.Pause();
        }

        private static void SearchAllOrdersFlow(UIContext context, DataTable dt)
        {
            string kw = ConsoleHelper.Prompt("Enter keyword (Customer Name / Status / Method)");
            DataTable results = SearchHelper.SearchDataTable(dt, kw);
            ShowFullDataTable(results);
            ConsoleHelper.Pause();
        }

        private static void FilterAllOrdersByStatusFlow(UIContext context, DataTable dt)
        {
            string status = ConsoleHelper.Prompt("Enter Status to filter (Pending/Completed/Cancelled)");
            DataTable results = SearchHelper.FilterByStatus(dt, status);
            ShowFullDataTable(results);
            ConsoleHelper.Pause();
        }

        private static void ShowFullDataTable(DataTable dt)
        {
            Console.WriteLine("\n--- SEARCH RESULTS ---");
            if (dt == null || dt.Rows.Count == 0) { Console.WriteLine("No records found."); return; }
            Console.WriteLine(" Order# | Customer             | Date       | Status    | Total");
            Console.WriteLine(" ------------------------------------------------------------------");
            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine($" {row["OrderID"],-6} | {row["Customer"],-20} | {Convert.ToDateTime(row["OrderDate"]):MM/dd/yy} | {row["Status"],-9} | Rs. {Convert.ToDecimal(row["GrantTotal"]):0.00}");
            }
        }
    }
}
