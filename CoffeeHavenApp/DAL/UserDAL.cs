using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB
{
    /// <summary>
    /// UserDAL handles account creation, authentication, and loyalty points.
    /// Updated to implement IUserRepository for Lab 06.
    /// </summary>
    public class UserDAL : IUserRepository
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\CoffeeHavenDB.mdf;Initial Catalog=CoffeeHavenDB;Integrated Security=True;Encrypt=False";

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

        public bool Register(string fullName, string email, string password)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (FullName, Email, PasswordHash, LoyaltyPoints) VALUES (@name, @email, @pass, 0)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", (object)password ?? DBNull.Value);

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
    }
}