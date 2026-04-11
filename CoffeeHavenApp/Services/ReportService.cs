using System.Data;
using CoffeeHavenDB.Interfaces;

namespace CoffeeHavenDB.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public DataTable GetSalesSummary()
        {
            return _reportRepository.GetSalesSummary();
        }

        public DataTable GetTopSellingProducts(int topN = 5)
        {
            return _reportRepository.GetTopSellingProducts(topN);
        }

        public DataTable GetInventoryHealth(int threshold = 5)
        {
            return _reportRepository.GetInventoryHealth(threshold);
        }

        public DataTable GetCustomerInsights(int topN = 5)
        {
            return _reportRepository.GetCustomerInsights(topN);
        }

        public DataTable GetOrderStatusAnalytics()
        {
            return _reportRepository.GetOrderStatusAnalytics();
        }
    }
}
