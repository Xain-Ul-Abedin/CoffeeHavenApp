using CoffeeHavenApp.Models;
using CoffeeHavenApp.UI;
using CoffeeHavenApp.UI.Base;
using CoffeeHavenDB.DAL;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Services;
using CoffeeHavenDB;
using CoffeeHavenApp.Services;
using System;

namespace CoffeeHavenApp
{
    /// <summary>
    /// Coffee Haven Console Application
    /// Entry Point — Orchestrates initialization and hands over to UI modules.
    /// </summary>
    class App
    {
        static void Main(string[] args)
        {
            // Set up database directory for local MDF file
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            // 1. Initialize DAL (Data Access Layer)
            IProductRepository productRepo = new ProductDAL();
            IOrderRepository orderRepo = new OrderDAL();
            IUserRepository userRepo = new UserDAL();
            IInventoryRepository inventoryRepo = new InventoryDAL();
            IReportRepository reportRepo = new ReportDAL();

            // 2. Initialize BLL (Business Logic Layer / Services)
            IProductService productService = new ProductService(productRepo);
            IOrderService orderService = new OrderService(orderRepo, productService);
            IUserService userService = new UserService(userRepo);
            IInventoryService inventoryService = new InventoryService(inventoryRepo);
            IReportService reportService = new ReportService(reportRepo);

            // 3. Initialize Shared State (Session & UI Context)
            Session session = new Session();
            UIContext context = new UIContext(userService, productService, orderService, inventoryService, reportService, session);

            // 4. Configure Console
            Console.Title = "Coffee Haven - POS System";

            // 5. Automated Testing Hook (for CI/CD)
            if (args.Length > 0 && args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
            {
                bool success = CoffeeHavenApp.Testing.TestRunner.RunAll(isCi: true);
                Environment.Exit(success ? 0 : 1);
            }

            // 6. Start Application Flow
            AuthUI.ShowLoginMenu(context);
        }
    }
}