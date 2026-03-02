using System;
using System.Text.RegularExpressions;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenDB.Services
{
    /// <summary>
    /// Lab 07 Implementation: UserService BLL
    /// Handles input sanitization, email format validation, and security guardrails.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int Login(string email, string password)
        {
            // [LOGIC] Input Sanitization
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Login failed. Email and Password cannot be empty.");
                return -1;
            }

            Console.WriteLine($"[BLL] Attempting authentication for: {email.Trim()}");
            return _userRepository.Login(email.Trim(), password);
        }

        public bool Register(string fullName, string email, string password)
        {
            Console.WriteLine($"[BLL] Validating registration for: {email}");

            // 1. [LOGIC] Mandatory Field Validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("[BLL] ERROR: Registration failed. All fields (Name, Email, Password) are mandatory.");
                return false;
            }

            // 2. [LOGIC] Email Format Validation using Regular Expressions
            if (!IsValidEmail(email))
            {
                Console.WriteLine("[BLL] ERROR: Invalid Email Format! Registration blocked.");
                return false;
            }

            // 3. [LOGIC] Password Strength Gatekeeper
            if (password.Length < 6)
            {
                Console.WriteLine("[BLL] ERROR: Password must be at least 6 characters long.");
                return false;
            }

            // 4. [DAL] Hand over sanitized data to the repository
            return _userRepository.Register(fullName.Trim(), email.Trim(), password);
        }

        public int GetPoints(int userId)
        {
            if (userId <= 0) return 0;
            return _userRepository.GetPoints(userId);
        }

        // Private Helper: Email Validation Regex
        private bool IsValidEmail(string email)
        {
            try
            {
                // Standard email pattern check
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