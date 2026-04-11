using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    public interface IReportService
    {
        DataTable GetSalesSummary();
        DataTable GetTopSellingProducts(int topN = 5);
        DataTable GetInventoryHealth(int threshold = 5);
        DataTable GetCustomerInsights(int topN = 5);
        DataTable GetOrderStatusAnalytics();
    }
}
