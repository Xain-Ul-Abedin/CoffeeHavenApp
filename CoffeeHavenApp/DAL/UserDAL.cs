using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB
{
    /// <summary>
    /// UserDAL handles account creation, authentication, profile management, and loyalty points.
    /// Updated for Role-Based Access Control.
    /// </summary>
    public class UserDAL : IUserRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

        // ================================================================
        // AUTHENTICATION
        // ================================================================

        /// <summary>
        /// Original login (no role check) — kept for backward compatibility.
        /// </summary>
        public int Login(string email, string password)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID FROM Users WHERE Email = @email AND (PasswordHash = @pass OR PasswordHash IS NULL)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", (object)password ?? DBNull.Value);

                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) return (int)result;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[LOGIN ERROR] " + ex.Message);
                }
            }
            return -1;
        }

        /// <summary>
        /// Role-aware login: validates email + password + role.
        /// Returns UserID on success, -1 on failure.
        /// </summary>
        public int Login(string email, string password, string role)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT UserID FROM Users 
                                 WHERE Email = @email 
                                   AND (PasswordHash = @pass OR PasswordHash IS NULL)
                                   AND Role = @role";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", (object)password ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@role", role);

                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) return (int)result;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[LOGIN ERROR] " + ex.Message);
                }
            }
            return -1;
        }

        /// <summary>
        /// Original register (defaults to Customer role).
        /// </summary>
        public bool Register(string fullName, string email, string password)
        {
            return Register(fullName, email, password, "Customer");
        }

        /// <summary>
        /// Role-aware register: inserts a new user with the specified role.
        /// </summary>
        public bool Register(string fullName, string email, string password, string role)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Users (FullName, Email, PasswordHash, LoyaltyPoints, Role) 
                                 VALUES (@name, @email, @pass, 0, @role)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", (object)password ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@role", role);

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                        Console.WriteLine("\n[REGISTRATION ERROR] Email already exists.");
                    else
                        Console.WriteLine("\n[REGISTRATION ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        // ================================================================
        // PROFILE MANAGEMENT
        // ================================================================

        public string GetUserFullName(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT FullName FROM Users WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    object res = cmd.ExecuteScalar();
                    return res != null ? res.ToString() : string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        public string GetUserEmail(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Email FROM Users WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    object res = cmd.ExecuteScalar();
                    return res != null ? res.ToString() : string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        public int GetPoints(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT LoyaltyPoints FROM Users WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    object res = cmd.ExecuteScalar();
                    return res != null ? (int)res : 0;
                }
                catch { return 0; }
            }
        }

        public bool UpdateFullName(int userId, string newName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET FullName = @name WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[UPDATE ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Verifies that the given password matches the stored hash for the user.
        /// </summary>
        public bool VerifyPassword(int userId, string password)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE UserID = @uId AND (PasswordHash = @pass OR PasswordHash IS NULL)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                cmd.Parameters.AddWithValue("@pass", (object)password ?? DBNull.Value);
                try
                {
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// Changes the password after verifying the current one.
        /// </summary>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            // First verify the current password
            if (!VerifyPassword(userId, currentPassword))
                return false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET PasswordHash = @newPass WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@newPass", newPassword);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[PASSWORD CHANGE ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Permanently deletes a user account. Orders remain for audit history.
        /// </summary>
        public bool DeleteAccount(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Users WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[DELETE ACCOUNT ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        // ================================================================
        // ADMIN: CUSTOMER MANAGEMENT
        // ================================================================

        /// <summary>
        /// Returns a DataTable of all users with their key fields.
        /// </summary>
        public DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, FullName, Email, Role, LoyaltyPoints FROM Users ORDER BY UserID";
                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                try
                {
                    con.Open();
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[GET USERS ERROR] " + ex.Message);
                }
            }
            return dt;
        }

        /// <summary>
        /// Returns a DataTable with PasswordHash for the debug accounts list.
        /// </summary>
        public DataTable GetDebugUserList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, FullName, Email, Role, PasswordHash FROM Users ORDER BY UserID";
                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                try
                {
                    con.Open();
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[GET DEBUG USERS ERROR] " + ex.Message);
                }
            }
            return dt;
        }

        public string GetUserRole(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Role FROM Users WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    object res = cmd.ExecuteScalar();
                    return res != null ? res.ToString() : string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        public bool UpdateUserRole(int userId, string newRole)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET Role = @role WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@role", newRole);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("\n[UPDATE ROLE ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        public bool UpdateUserEmail(int userId, string newEmail)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET Email = @email WHERE UserID = @uId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", newEmail);
                cmd.Parameters.AddWithValue("@uId", userId);
                try
                {
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601)
                        Console.WriteLine("\n[UPDATE EMAIL ERROR] Email already exists.");
                    else
                        Console.WriteLine("\n[UPDATE EMAIL ERROR] " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Admin-level delete: no password required.
        /// </summary>
        public bool AdminDeleteUser(int userId)
        {
            return DeleteAccount(userId); // reuses the same SQL
        }
    }
}