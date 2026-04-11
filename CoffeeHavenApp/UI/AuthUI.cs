using System;
using System.Data;
using CoffeeHavenApp.Testing;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenApp.UI
{
    public static class AuthUI
    {
        private const string AdminSecretKey = "ADMIN2026";

        public static void ShowLoginMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("WELCOME TO COFFEE HAVEN");
                Console.WriteLine(" 1. Login");
                Console.WriteLine(" 2. Register");
                Console.WriteLine(" 3. View Accounts List (Debug)");
                Console.WriteLine(" 4. Test & Verify (Debug)");
                Console.WriteLine(" 5. Exit");

                string choice = ConsoleHelper.Prompt("Selection").ToUpperInvariant();

                switch (choice)
                {
                    case "1":
                        if (PerformLogin(context))
                        {
                            DashboardUI.ShowMainMenu(context);
                        }
                        break;

                    case "2":
                        PerformRegistration(context);
                        break;

                    case "3":
                        ShowDebugAccountsList(context.UserService);
                        break;

                    case "4":
                        TestRunner.RunAll();
                        break;

                    case "5":
                        Environment.Exit(0);
                        return;

                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection. Please choose 1, 2, 3, 4, or 5.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private static bool PerformLogin(UIContext context)
        {
            ConsoleHelper.DrawHeader("LOGIN");

            Console.WriteLine(" Login as:");
            Console.WriteLine(" 1. Admin");
            Console.WriteLine(" 2. Customer");

            string roleChoice = ConsoleHelper.Prompt("Selection");
            string role = roleChoice == "1" ? "Admin" : (roleChoice == "2" ? "Customer" : string.Empty);

            if (string.IsNullOrEmpty(role))
            {
                ConsoleHelper.ErrorMessage("Invalid selection.");
                ConsoleHelper.Pause();
                return false;
            }

            string email = ConsoleHelper.Prompt("Email");
            string password = ConsoleHelper.Prompt("Password");

            int userId = context.UserService.Login(email, password, role);
            if (userId > 0)
            {
                context.Session.UserId = userId;
                context.Session.UserEmail = email.Trim();
                context.Session.UserRole = role;

                string fullName = context.UserService.GetUserFullName(userId);
                context.Session.UserName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName
                    : (email.Contains("@") ? email.Split('@')[0] : email);

                ConsoleHelper.SuccessMessage($"Login successful. Welcome, {context.Session.UserName}! [{context.Session.UserRole}]");
                ConsoleHelper.Pause();
                return true;
            }

            ConsoleHelper.ErrorMessage($"Login failed. No {role} account found with those credentials.");
            ConsoleHelper.Pause();
            return false;
        }

        private static void PerformRegistration(UIContext context)
        {
            ConsoleHelper.DrawHeader("REGISTER");

            Console.WriteLine(" Register as:");
            Console.WriteLine(" 1. Admin");
            Console.WriteLine(" 2. Customer");

            string roleChoice = ConsoleHelper.Prompt("Selection");
            string role = roleChoice == "1" ? "Admin" : (roleChoice == "2" ? "Customer" : string.Empty);

            if (string.IsNullOrEmpty(role))
            {
                ConsoleHelper.ErrorMessage("Invalid selection.");
                ConsoleHelper.Pause();
                return;
            }

            if (role == "Admin")
            {
                string secretKey = ConsoleHelper.Prompt("Enter Admin Secret Key");
                if (!secretKey.Equals(AdminSecretKey, StringComparison.Ordinal))
                {
                    ConsoleHelper.ErrorMessage("Invalid secret key. Admin registration denied.");
                    ConsoleHelper.Pause();
                    return;
                }
            }

            string fullName = ConsoleHelper.Prompt("Full Name");
            string email = ConsoleHelper.Prompt("Email");
            string password = ConsoleHelper.Prompt("Password");

            if (context.UserService.Register(fullName, email, password, role))
                ConsoleHelper.SuccessMessage($"{role} registration successful. You can now log in.");
            else
                ConsoleHelper.ErrorMessage("Registration failed.");

            ConsoleHelper.Pause();
        }

        private static void ShowDebugAccountsList(IUserService userService)
        {
            ConsoleHelper.DrawHeader("ACCOUNTS LIST (DEBUG)");
            DataTable dt = userService.GetDebugUserList();

            if (dt == null || dt.Rows.Count == 0)
            {
                ConsoleHelper.InfoMessage("No users found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("\n ID  | Role     | Email                  | Password");
            Console.WriteLine(" ---------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                string role = row["Role"].ToString();
                string email = row["Email"].ToString();
                string pass = row["PasswordHash"] == DBNull.Value ? "(NULL)" : row["PasswordHash"].ToString();

                Console.WriteLine($" {userId,-3} | {role,-8} | {email,-22} | {pass}");
            }
            ConsoleHelper.Pause();
        }
    }
}
