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

            // 2. Initialize BLL (Business Logic Layer / Services)
            IProductService productService = new ProductService(productRepo);
            IOrderService orderService = new OrderService(orderRepo, productService);
            IUserService userService = new UserService(userRepo);
            IInventoryService inventoryService = new InventoryService(inventoryRepo);

            // 3. Initialize Shared State (Session & UI Context)
            Session session = new Session();
            UIContext context = new UIContext(userService, productService, orderService, inventoryService, session);

            // 4. Configure Console
            Console.Title = "Coffee Haven - POS System";

            // 5. Start Application Flow
            AuthUI.ShowLoginMenu(context);
        }
    }
}