using System;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenApp.UI.Admin;
using CoffeeHavenApp.UI.Customer;

namespace CoffeeHavenApp.UI
{
    public static class DashboardUI
    {
        public static void ShowMainMenu(UIContext context)
        {
            if (context.Session.IsAdmin)
                ShowAdminDashboard(context);
            else
                ShowCustomerDashboard(context);
        }

        private static void ShowAdminDashboard(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader($"ADMIN DASHBOARD | {context.Session.UserName.ToUpperInvariant()}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  Role: Administrator");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(" 1. Order Management");
                Console.WriteLine(" 2. Product Management");
                Console.WriteLine(" 3. Inventory Management");
                Console.WriteLine(" 4. Customer Management");
                Console.WriteLine(" 5. Settings");
                Console.WriteLine(" 6. Logout");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        AdminOrderManagementUI.ShowAdminOrderMenu(context);
                        break;
                    case "2":
                        ProductManagementUI.ShowProductManagementMenu(context);
                        break;
                    case "3":
                        InventoryManagementUI.ShowInventoryManagementMenu(context);
                        break;
                    case "4":
                        CustomerManagementUI.ShowCustomerManagementMenu(context);
                        break;
                    case "5":
                        if (ProfileUI.ShowProfileMenu(context)) return;
                        break;
                    case "6":
                        if (ConfirmLogout(context)) return;
                        break;
                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private static void ShowCustomerDashboard(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader($"WELCOME, {context.Session.UserName.ToUpperInvariant()}!");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("  Role: Customer");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(" 1. Shop");
                Console.WriteLine(" 2. My Profile");
                Console.WriteLine(" 3. Logout");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        OrderManagementUI.ShowOrderManagementMenu(context);
                        break;
                    case "2":
                        if (ProfileUI.ShowProfileMenu(context)) return;
                        break;
                    case "3":
                        if (ConfirmLogout(context)) return;
                        break;
                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private static bool ConfirmLogout(UIContext context)
        {
            if (ConsoleHelper.Confirm("\nAre you sure you want to logout? (y/n): "))
            {
                context.Session.Clear();
                ConsoleHelper.SuccessMessage("Logged out successfully.");
                ConsoleHelper.Pause();
                return true;
            }
            return false;
        }
    }
}
