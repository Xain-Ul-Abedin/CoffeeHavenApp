using System;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenApp.UI.Helpers;

namespace CoffeeHavenApp.UI
{
    public static class ProfileUI
    {
        public static bool ShowProfileMenu(UIContext context)
        {
            while (true)
            {
                ConsoleHelper.DrawHeader("MY PROFILE");
                Console.WriteLine(" 1. View Profile");
                Console.WriteLine(" 2. Update Name");
                Console.WriteLine(" 3. Update Email");
                Console.WriteLine(" 4. Change Password");
                Console.WriteLine(" 5. Delete Account");
                Console.WriteLine(" 6. Back");

                string choice = ConsoleHelper.Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        ViewProfile(context);
                        ConsoleHelper.Pause();
                        break;
                    case "2":
                        UpdateNameFlow(context);
                        break;
                    case "3":
                        UpdateEmailFlow(context);
                        break;
                    case "4":
                        ChangePasswordFlow(context);
                        break;
                    case "5":
                        if (DeleteAccountFlow(context)) return true;
                        break;
                    case "6":
                        return false;
                    default:
                        ConsoleHelper.ErrorMessage("Invalid selection.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private static void ViewProfile(UIContext context)
        {
            ConsoleHelper.DrawHeader("PROFILE INFO");
            string fullName = context.UserService.GetUserFullName(context.Session.UserId);
            string email = context.UserService.GetUserEmail(context.Session.UserId);
            int points = context.UserService.GetPoints(context.Session.UserId);

            Console.WriteLine();
            Console.WriteLine($"  Name           : {fullName}");
            Console.WriteLine($"  Email          : {email}");
            Console.WriteLine($"  Role           : {context.Session.UserRole}");
            Console.WriteLine($"  Loyalty Points : {points}");
        }

        private static void UpdateNameFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("UPDATE NAME");
            string currentName = context.UserService.GetUserFullName(context.Session.UserId);
            Console.WriteLine($"\n  Current Name: {currentName}");

            string newName = ConsoleHelper.Prompt("Enter new name");
            if (string.IsNullOrWhiteSpace(newName))
            {
                ConsoleHelper.InfoMessage("No changes made.");
                ConsoleHelper.Pause();
                return;
            }

            if (context.UserService.UpdateFullName(context.Session.UserId, newName))
            {
                context.Session.UserName = newName;
                ConsoleHelper.SuccessMessage("Name updated successfully.");
            }
            else
            {
                ConsoleHelper.ErrorMessage("Failed to update name.");
            }
            ConsoleHelper.Pause();
        }

        private static void UpdateEmailFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("UPDATE EMAIL");
            string currentEmail = context.UserService.GetUserEmail(context.Session.UserId);
            Console.WriteLine($"\n  Current Email: {currentEmail}");

            string newEmail = ConsoleHelper.Prompt("Enter new email");
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                ConsoleHelper.InfoMessage("No changes made.");
                ConsoleHelper.Pause();
                return;
            }

            if (context.UserService.UpdateUserEmail(context.Session.UserId, newEmail))
            {
                context.Session.UserEmail = newEmail;
                ConsoleHelper.SuccessMessage("Email updated successfully.");
            }
            else
            {
                ConsoleHelper.ErrorMessage("Failed to update email.");
            }
            ConsoleHelper.Pause();
        }

        private static void ChangePasswordFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("CHANGE PASSWORD");
            string currentPassword = ConsoleHelper.Prompt("Enter current password");

            if (!context.UserService.VerifyPassword(context.Session.UserId, currentPassword))
            {
                ConsoleHelper.ErrorMessage("Current password is incorrect.");
                ConsoleHelper.Pause();
                return;
            }

            string newPassword = ConsoleHelper.Prompt("Enter new password (min 6 characters)");
            string confirmPassword = ConsoleHelper.Prompt("Confirm new password");

            if (newPassword != confirmPassword)
            {
                ConsoleHelper.ErrorMessage("Passwords do not match.");
                ConsoleHelper.Pause();
                return;
            }

            if (context.UserService.ChangePassword(context.Session.UserId, currentPassword, newPassword))
                ConsoleHelper.SuccessMessage("Password changed successfully.");
            else
                ConsoleHelper.ErrorMessage("Password change failed. Check requirements.");

            ConsoleHelper.Pause();
        }

        private static bool DeleteAccountFlow(UIContext context)
        {
            ConsoleHelper.DrawHeader("DELETE ACCOUNT");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  WARNING: This action is permanent and cannot be undone.");
            Console.WriteLine("  All your account data will be removed.");
            Console.ResetColor();

            Console.Write("\n  Type DELETE to confirm: ");
            string confirm = Console.ReadLine();

            if (confirm == null || !confirm.Trim().Equals("DELETE", StringComparison.Ordinal))
            {
                ConsoleHelper.InfoMessage("Account deletion cancelled.");
                ConsoleHelper.Pause();
                return false;
            }

            string password = ConsoleHelper.Prompt("Enter your password to confirm");
            if (context.UserService.DeleteAccount(context.Session.UserId, password))
            {
                ConsoleHelper.SuccessMessage("Account deleted. Returning to welcome screen.");
                ConsoleHelper.Pause();
                context.Session.Clear();
                return true;
            }
            else
            {
                ConsoleHelper.ErrorMessage("Account deletion failed. Password may be incorrect.");
                ConsoleHelper.Pause();
                return false;
            }
        }
    }
}
