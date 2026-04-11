namespace CoffeeHavenApp.Testing.Base
{
    public interface ITestCase
    {
        string Name { get; }
        bool Run();
        string ErrorMessage { get; }
    }
}
