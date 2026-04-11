using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;

namespace CoffeeHavenApp.Testing.Mocks
{
    public class InMemoryUserDAL : IUserRepository
    {
        private List<User> _users = new List<User>();
        private int _nextId = 1;

        public void Reset()
        {
            _users.Clear();
            _nextId = 1;
        }

        public int Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            return user != null ? user.UserId : -1;
        }

        public int Login(string email, string password, string role)
        {
            var user = _users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password && u.Role == role);
            return user != null ? user.UserId : -1;
        }

        public bool Register(string fullName, string email, string password)
        {
            return Register(fullName, email, password, "Customer");
        }

        public bool Register(string fullName, string email, string password, string role)
        {
            if (_users.Any(u => u.Email == email)) return false;

            _users.Add(new User
            {
                UserId = _nextId++,
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Role = role,
                LoyaltyPoints = 0
            });
            return true;
        }

        public string GetUserFullName(int userId) => _users.FirstOrDefault(u => u.UserId == userId)?.FullName ?? string.Empty;
        public string GetUserEmail(int userId) => _users.FirstOrDefault(u => u.UserId == userId)?.Email ?? string.Empty;
        public int GetPoints(int userId) => _users.FirstOrDefault(u => u.UserId == userId)?.LoyaltyPoints ?? 0;

        public bool UpdateFullName(int userId, string newName)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;
            user.FullName = newName;
            return true;
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId && u.PasswordHash == currentPassword);
            if (user == null) return false;
            user.PasswordHash = newPassword;
            return true;
        }

        public bool VerifyPassword(int userId, string password)
        {
            return _users.Any(u => u.UserId == userId && u.PasswordHash == password);
        }

        public bool DeleteAccount(int userId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;
            _users.Remove(user);
            return true;
        }

        public DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("UserID", typeof(int));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("LoyaltyPoints", typeof(int));

            foreach (var u in _users)
            {
                dt.Rows.Add(u.UserId, u.FullName, u.Email, u.Role, u.LoyaltyPoints);
            }
            return dt;
        }

        public DataTable GetDebugUserList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("UserID", typeof(int));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("PasswordHash", typeof(string));

            foreach (var u in _users)
            {
                dt.Rows.Add(u.UserId, u.FullName, u.Email, u.Role, u.PasswordHash);
            }
            return dt;
        }

        public string GetUserRole(int userId) => _users.FirstOrDefault(u => u.UserId == userId)?.Role ?? string.Empty;

        public bool UpdateUserRole(int userId, string newRole)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;
            user.Role = newRole;
            return true;
        }

        public bool UpdateUserEmail(int userId, string newEmail)
        {
            if (_users.Any(u => u.Email == newEmail && u.UserId != userId)) return false;
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;
            user.Email = newEmail;
            return true;
        }

        public bool AdminDeleteUser(int userId) => DeleteAccount(userId);
    }
}
