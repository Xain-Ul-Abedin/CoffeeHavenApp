using System;
using System.Data;
using CoffeeHavenApp.Helpers;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenApp.UI.Admin
{
    public static class InventoryManagementUI
    {
        public static void ShowInventoryManagementMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("INVENTORY MANAGEMENT");
                Console.WriteLine(" 1. View Low Stock Items");
                Console.WriteLine(" 2. Search Inventory");
                Console.WriteLine(" 3. Restock Item");
                Console.WriteLine(" 4. Back");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        int threshold = ConsoleHelper.SafeReadInt("Low stock threshold");
                        if (threshold <= 0) threshold = 10;
                        DisplayInventoryTable(context.InventoryService.GetLowStockItems(threshold));
                        ConsoleHelper.Pause();
                        break;
                    case "2":
                        SearchInventoryFlow(context);
                        break;
                    case "3":
                        RestockFlow(context);
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

        public static void DisplayInventoryTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                Console.WriteLine("No matching inventory records found.");
                return;
            }

            Console.WriteLine("\n ID | Product Name         | Current Stock");
            Console.WriteLine("-------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine($" {row["ItemID"],-2} | {row["Name"],-20} | {row["StockQuantity"]}");
            }
        }

        private static void SearchInventoryFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("SEARCH INVENTORY");
            DataTable dt = context.InventoryService.GetLowStockItems(999999);
            if (dt == null || dt.Rows.Count == 0) { ConsoleHelper.InfoMessage("No records found."); ConsoleHelper.Pause(); return; }

            string keyword = ConsoleHelper.Prompt("Enter keyword");
            DataTable filtered = SearchHelper.SearchDataTable(dt, keyword);

            if (filtered.Rows.Count == 0) { ConsoleHelper.InfoMessage("No matches."); }
            else { DisplayInventoryTable(filtered); }
            ConsoleHelper.Pause();
        }

        private static void RestockFlow(UIContext context)
        {
            DisplayInventoryTable(context.InventoryService.GetLowStockItems(999999));
            int id = ConsoleHelper.SafeReadInt("\nEnter Item ID to restock");
            int qty = ConsoleHelper.SafeReadInt("Enter quantity to add");
            
            if (id > 0 && qty > 0)
            {
                context.InventoryService.RestockItem(id, qty);
                ConsoleHelper.SuccessMessage("Restock completed.");
            }
            else
            {
                ConsoleHelper.ErrorMessage("Invalid ID or Quantity.");
            }
            ConsoleHelper.Pause();
        }
    }
}
