namespace CoffeeHavenDB.Interfaces
{
    // Part of Lab 6: Contract for User Business Logic
    public interface IUserService
    {
        int Login(string email, string password);
        bool Register(string fullName, string email, string password);
        int GetPoints(int userId);
    }
}