using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    /// <summary>
    /// Contract for User Business Logic.
    /// Updated for Role-Based Access Control and Profile Management.
    /// </summary>
    public interface IUserService
    {
        // ── Authentication ────────────────────────────────────────
        int Login(string email, string password);
        int Login(string email, string password, string role);
        bool Register(string fullName, string email, string password);
        bool Register(string fullName, string email, string password, string role);

        // ── Profile ───────────────────────────────────────────────
        string GetUserFullName(int userId);
        string GetUserEmail(int userId);
        int GetPoints(int userId);
        bool UpdateFullName(int userId, string newName);
        bool ChangePassword(int userId, string currentPassword, string newPassword);
        bool VerifyPassword(int userId, string password);
        bool DeleteAccount(int userId, string password);

        // ── Admin: Customer Management ────────────────────────────
        DataTable GetAllUsers();
        DataTable GetDebugUserList();
        string GetUserRole(int userId);
        bool UpdateUserRole(int userId, string newRole);
        bool UpdateUserEmail(int userId, string newEmail);
        bool AdminDeleteUser(int userId);
    }
}