using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    // Part of Lab 6: Contract for User Data Access
    public interface IUserRepository
    {
        int Login(string email, string password);
        bool Register(string fullName, string email, string password);
        int GetPoints(int userId);
    }
}