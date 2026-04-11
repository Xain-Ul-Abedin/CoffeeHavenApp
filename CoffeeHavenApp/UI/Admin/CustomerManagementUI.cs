using System;
using System.Data;
using CoffeeHavenApp.Helpers;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenApp.UI.Admin
{
    public static class CustomerManagementUI
    {
        public static void ShowCustomerManagementMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("CUSTOMER MANAGEMENT");
                Console.WriteLine(" 1. View All Users");
                Console.WriteLine(" 2. Search / Filter Users");
                Console.WriteLine(" 3. Add User");
                Console.WriteLine(" 4. Update User");
                Console.WriteLine(" 5. Delete User");
                Console.WriteLine(" 6. Back");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplayUserTable(context.UserService.GetAllUsers());
                        ConsoleHelper.Pause();
                        break;
                    case "2":
                        SearchUsersFlow(context);
                        break;
                    case "3":
                        AdminAddUserFlow(context);
                        break;
                    case "4":
                        AdminUpdateUserFlow(context);
                        break;
                    case "5":
                        AdminDeleteUserFlow(context);
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

        public static void DisplayUserTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                ConsoleHelper.InfoMessage("No users found.");
                return;
            }

            Console.WriteLine("\n ID  | Name                 | Email                  | Role     | Points");
            Console.WriteLine(" --------------------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                string name = row["FullName"].ToString();
                string email = row["Email"].ToString();
                string role = row["Role"].ToString();
                int points = row["LoyaltyPoints"] == DBNull.Value ? 0 : Convert.ToInt32(row["LoyaltyPoints"]);

                Console.WriteLine($" {userId,-3} | {name,-20} | {email,-22} | {role,-8} | {points}");
            }
        }

        private static void SearchUsersFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("SEARCH / FILTER USERS");
            DataTable dt = context.UserService.GetAllUsers();
            if (dt == null || dt.Rows.Count == 0) { ConsoleHelper.InfoMessage("No users to search."); ConsoleHelper.Pause(); return; }

            Console.WriteLine(" Filter by: 1. Keyword  2. Role");
            string filterChoice = ConsoleHelper.Prompt("Selection");
            DataTable filtered;

            if (filterChoice == "1")
            {
                string keyword = ConsoleHelper.Prompt("Enter search keyword");
                filtered = SearchHelper.SearchDataTable(dt, keyword);
            }
            else if (filterChoice == "2")
            {
                Console.WriteLine("\n 1. Admin  2. Customer");
                string roleChoice = ConsoleHelper.Prompt("Selection");
                string role = roleChoice == "1" ? "Admin" : "Customer";
                filtered = SearchHelper.FilterDataTableByColumn(dt, "Role", role);
            }
            else
            {
                ConsoleHelper.ErrorMessage("Invalid selection."); ConsoleHelper.Pause(); return;
            }

            if (filtered.Rows.Count == 0) { ConsoleHelper.InfoMessage("No users matched."); }
            else { DisplayUserTable(filtered); }
            ConsoleHelper.Pause();
        }

        private static void AdminAddUserFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("ADD USER");
            Console.WriteLine(" Role: 1. Admin  2. Customer");
            string roleChoice = ConsoleHelper.Prompt("Selection");
            string role = roleChoice == "1" ? "Admin" : (roleChoice == "2" ? "Customer" : string.Empty);

            if (string.IsNullOrEmpty(role)) { ConsoleHelper.ErrorMessage("Invalid role."); ConsoleHelper.Pause(); return; }

            string name = ConsoleHelper.Prompt("Full Name");
            string email = ConsoleHelper.Prompt("Email");
            string pass = ConsoleHelper.Prompt("Password");

            if (context.UserService.Register(name, email, pass, role))
                ConsoleHelper.SuccessMessage($"{role} created successfully.");
            else
                ConsoleHelper.ErrorMessage("Failed to create user.");
            ConsoleHelper.Pause();
        }

        private static void AdminUpdateUserFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("UPDATE USER");
            DisplayUserTable(context.UserService.GetAllUsers());

            int id = ConsoleHelper.SafeReadInt("\nEnter User ID to update");
            string existingName = context.UserService.GetUserFullName(id);
            if (string.IsNullOrEmpty(existingName)) { ConsoleHelper.ErrorMessage("User not found."); ConsoleHelper.Pause(); return; }

            Console.WriteLine("\n 1. Name  2. Email  3. Role");
            string choice = ConsoleHelper.Prompt("Update field");

            switch (choice)
            {
                case "1":
                    string n = ConsoleHelper.Prompt("New Name");
                    if (context.UserService.UpdateFullName(id, n)) ConsoleHelper.SuccessMessage("Name updated.");
                    break;
                case "2":
                    string e = ConsoleHelper.Prompt("New Email");
                    if (context.UserService.UpdateUserEmail(id, e)) ConsoleHelper.SuccessMessage("Email updated.");
                    break;
                case "3":
                    Console.WriteLine("\n 1. Admin  2. Customer");
                    string r = ConsoleHelper.Prompt("New Role") == "1" ? "Admin" : "Customer";
                    if (context.UserService.UpdateUserRole(id, r)) ConsoleHelper.SuccessMessage("Role updated.");
                    break;
            }
            ConsoleHelper.Pause();
        }

        private static void AdminDeleteUserFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("DELETE USER");
            DisplayUserTable(context.UserService.GetAllUsers());
            int id = ConsoleHelper.SafeReadInt("\nEnter User ID to delete");

            if (id == context.Session.UserId) { ConsoleHelper.ErrorMessage("Cannot delete yourself here."); ConsoleHelper.Pause(); return; }

            if (ConsoleHelper.Confirm("Are you sure? Type YES: "))
            {
                if (context.UserService.AdminDeleteUser(id)) ConsoleHelper.SuccessMessage("User deleted.");
                else ConsoleHelper.ErrorMessage("Failed to delete user.");
            }
            ConsoleHelper.Pause();
        }
    }
}
