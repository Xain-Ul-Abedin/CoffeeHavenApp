using System.Data;

namespace CoffeeHavenDB.Interfaces
{
    public interface IReportRepository
    {
        DataTable GetSalesSummary();
        DataTable GetTopSellingProducts(int topN);
        DataTable GetInventoryHealth(int lowStockThreshold);
        DataTable GetCustomerInsights(int topN);
        DataTable GetOrderStatusAnalytics();
    }
}
