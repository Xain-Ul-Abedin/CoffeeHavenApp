using System;
using System.Data;
using System.Text.RegularExpressions;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenDB.Services
{
    /// <summary>
    /// UserService BLL — handles input sanitization, validation, and security guardrails.
    /// Updated for Role-Based Access Control and Profile Management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // ================================================================
        // AUTHENTICATION
        // ================================================================

        public int Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Login failed. Email and Password cannot be empty.");
                return -1;
            }

            Console.WriteLine($"[BLL] Attempting authentication for: {email.Trim()}");
            return _userRepository.Login(email.Trim(), password);
        }

        /// <summary>
        /// Role-aware login: validates credentials AND checks the stored role matches.
        /// </summary>
        public int Login(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Login failed. Email and Password cannot be empty.");
                return -1;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                Console.WriteLine("[BLL] ERROR: Login failed. Role must be specified.");
                return -1;
            }

            Console.WriteLine($"[BLL] Attempting {role} authentication for: {email.Trim()}");
            return _userRepository.Login(email.Trim(), password, role.Trim());
        }

        public bool Register(string fullName, string email, string password)
        {
            return Register(fullName, email, password, "Customer");
        }

        /// <summary>
        /// Role-aware registration with full validation.
        /// </summary>
        public bool Register(string fullName, string email, string password, string role)
        {
            Console.WriteLine($"[BLL] Validating {role} registration for: {email}");

            // 1. Mandatory Field Validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Registration failed. All fields (Name, Email, Password) are mandatory.");
                return false;
            }

            // 2. Email Format Validation
            if (!IsValidEmail(email))
            {
                Console.WriteLine("[BLL] ERROR: Invalid Email Format! Registration blocked.");
                return false;
            }

            // 3. Password Strength
            if (password.Length < 6)
            {
                Console.WriteLine("[BLL] ERROR: Password must be at least 6 characters long.");
                return false;
            }

            // 4. Role Validation
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[BLL] ERROR: Invalid role. Must be 'Admin' or 'Customer'.");
                return false;
            }

            // 5. Hand over to DAL
            return _userRepository.Register(fullName.Trim(), email.Trim(), password, role.Trim());
        }

        // ================================================================
        // PROFILE MANAGEMENT
        // ================================================================

        public string GetUserFullName(int userId)
        {
            if (userId <= 0) return string.Empty;
            return _userRepository.GetUserFullName(userId);
        }

        public string GetUserEmail(int userId)
        {
            if (userId <= 0) return string.Empty;
            return _userRepository.GetUserEmail(userId);
        }

        public int GetPoints(int userId)
        {
            if (userId <= 0) return 0;
            return _userRepository.GetPoints(userId);
        }

        public bool UpdateFullName(int userId, string newName)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("[BLL] ERROR: Name cannot be empty.");
                return false;
            }

            Console.WriteLine($"[BLL] Updating name for user #{userId}.");
            return _userRepository.UpdateFullName(userId, newName.Trim());
        }

        /// <summary>
        /// Secure password change: validates current password, enforces strength on new password.
        /// </summary>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                Console.WriteLine("[BLL] ERROR: Passwords cannot be empty.");
                return false;
            }

            if (newPassword.Length < 6)
            {
                Console.WriteLine("[BLL] ERROR: New password must be at least 6 characters long.");
                return false;
            }

            if (currentPassword == newPassword)
            {
                Console.WriteLine("[BLL] ERROR: New password must be different from current password.");
                return false;
            }

            return _userRepository.ChangePassword(userId, currentPassword, newPassword);
        }

        public bool VerifyPassword(int userId, string password)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(password)) return false;
            return _userRepository.VerifyPassword(userId, password);
        }

        /// <summary>
        /// Deletes account after password verification at the BLL level.
        /// </summary>
        public bool DeleteAccount(int userId, string password)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Password is required to delete account.");
                return false;
            }

            // Verify password before deletion
            if (!_userRepository.VerifyPassword(userId, password))
            {
                Console.WriteLine("[BLL] ERROR: Password verification failed. Account deletion blocked.");
                return false;
            }

            Console.WriteLine($"[BLL] Deleting account for user #{userId}.");
            return _userRepository.DeleteAccount(userId);
        }

        // ================================================================
        // ADMIN: CUSTOMER MANAGEMENT
        // ================================================================

        public DataTable GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }

        public DataTable GetDebugUserList()
        {
            return _userRepository.GetDebugUserList();
        }

        public string GetUserRole(int userId)
        {
            if (userId <= 0) return string.Empty;
            return _userRepository.GetUserRole(userId);
        }

        public bool UpdateUserRole(int userId, string newRole)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(newRole) ||
                (!newRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                 !newRole.Equals("Customer", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("[BLL] ERROR: Role must be 'Admin' or 'Customer'.");
                return false;
            }

            Console.WriteLine($"[BLL] Updating role for user #{userId} to {newRole}.");
            return _userRepository.UpdateUserRole(userId, newRole.Trim());
        }

        public bool UpdateUserEmail(int userId, string newEmail)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(newEmail) || !IsValidEmail(newEmail))
            {
                Console.WriteLine("[BLL] ERROR: Invalid email format.");
                return false;
            }

            Console.WriteLine($"[BLL] Updating email for user #{userId}.");
            return _userRepository.UpdateUserEmail(userId, newEmail.Trim());
        }

        public bool AdminDeleteUser(int userId)
        {
            if (userId <= 0)
            {
                Console.WriteLine("[BLL] ERROR: Invalid user ID.");
                return false;
            }

            Console.WriteLine($"[BLL] Admin deleting user #{userId}.");
            return _userRepository.AdminDeleteUser(userId);
        }

        // ================================================================
        // PRIVATE HELPERS
        // ================================================================

        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}