using CoffeeHavenApp.Services;
using CoffeeHavenDB;
using CoffeeHavenDB.DAL;
using CoffeeHavenDB.Interfaces;
using CoffeeHavenDB.Models;
using CoffeeHavenDB.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CoffeeHavenApp
{
    /// <summary>
    /// COFFEE HAVEN PLATFORM - LAB 07 FINAL VERSION
    /// -------------------------------------------
    /// UX Priorities: Context-aware lists, Safe Inputs, and Clear Headers.
    /// Logic: Full Business Logic Layer (BLL) with Dependency Injection.
    /// </summary>
    class App
    {
        // Global Session State
        static int loggedInUserId = -1;
        static string loggedInUserName = "";

        static void Main(string[] args)
        {
            // Portability for LocalDB
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            // 1. Initialize Repositories (DAL)
            IProductRepository productRepo = new ProductDAL();
            IOrderRepository orderRepo = new OrderDAL();
            IUserRepository userRepo = new UserDAL();
            IInventoryRepository inventoryRepo = new InventoryDAL();

            // 2. Initialize Services (BLL)
            // Note: OrderService requires ProductService for stock orchestration (Lab 07)
            IProductService productService = new ProductService(productRepo);
            IOrderService orderService = new OrderService(orderRepo, productService);
            IUserService userService = new UserService(userRepo);
            IInventoryService inventoryService = new InventoryService(inventoryRepo);

            Console.Title = "Coffee Haven Enterprise - BLL Secured";

            // Runs diagnostic logic check quietly at startup
            RunSilentDiagnostic(productService);

            // 5. Entry Menu
            ShowLoginMenu(userService, productService, orderService, inventoryService);
        }

        #region Diagnostic Phase
        static void RunSilentDiagnostic(IProductService ps)
        {
            DrawHeader("SYSTEM BOOT");
            Console.WriteLine("[BLL] Verifying Business Logic Guardrails...");
            // Silently attempt an invalid product to ensure BLL is active
            ps.AddProduct(new Product { ProductName = "", Price = -1 });
            Console.WriteLine("\n[SYS] Services initialized. Press any key to start...");
            Console.ReadKey();
        }
        #endregion

        #region Main Navigation Menus
        static void ShowLoginMenu(IUserService uServ, IProductService pServ, IOrderService oServ, IInventoryService iServ)
        {
            while (true)
            {
                DrawHeader("WELCOME TO COFFEE HAVEN");
                Console.WriteLine(" 1. > Login to Account");
                Console.WriteLine(" 2. > Register New Membership");
                Console.WriteLine(" 3. > Exit Application");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        if (PerformLogin(uServ)) ShowMainMenu(pServ, oServ, iServ, uServ);
                        break;
                    case "2":
                        PerformRegistration(uServ);
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        NotifyError("Invalid selection. Use 1, 2, or 3.");
                        break;
                }
            }
        }

        static void ShowMainMenu(IProductService pServ, IOrderService oServ, IInventoryService iServ, IUserService uServ)
        {
            while (true)
            {
                DrawHeader($"DASHBOARD | {loggedInUserName.ToUpper()}");
                Console.WriteLine($" Loyalty Points: {uServ.GetPoints(loggedInUserId)} | ID: {loggedInUserId}");
                Console.WriteLine(" -------------------------------------------------------------");
                Console.WriteLine(" 1. [STORE]   Browse Menu & Order");
                Console.WriteLine(" 2. [ORDERS]  My History & Cancellations");
                Console.WriteLine(" 3. [ADMIN]   Product & Stock Control");
                Console.WriteLine(" 4. [LOGOUT]  Exit to Login Screen");

                switch (Prompt("Dashboard"))
                {
                    case "1": ShowOrderMenu(oServ, pServ); break;
                    case "2": ShowHistoryMenu(oServ); break;
                    case "3": ShowAdminModule(pServ, iServ); break;
                    case "4": loggedInUserId = -1; return;
                    default: NotifyError("Selection not recognized."); break;
                }
            }
        }

        static void ShowAdminModule(IProductService pServ, IInventoryService iServ)
        {
            while (true)
            {
                DrawHeader("ADMINISTRATION CONSOLE");
                Console.WriteLine(" 1. Product Catalog Management");
                Console.WriteLine(" 2. Inventory & Stock Audit");
                Console.WriteLine(" 3. Back to Dashboard");

                string choice = Prompt("Admin Tool");
                if (choice == "1") ShowProductMgmt(pServ);
                else if (choice == "2") ShowInventoryMgmt(iServ);
                else if (choice == "3") break;
            }
        }
        #endregion

        #region Functional UX Flows (Fixing Logic Gaps)
        static void ShowOrderMenu(IOrderService oServ, IProductService pServ)
        {
            while (true)
            {
                DrawHeader("MENU & ORDERING");
                Console.WriteLine(" 1. Order Single Item");
                Console.WriteLine(" 2. Multi-Item Checkout (Cart)");
                Console.WriteLine(" 3. View Full Menu List");
                Console.WriteLine(" 4. Back");

                string choice = Prompt("Selection");
                if (choice == "4") break;

                switch (choice)
                {
                    case "1": PerformPlaceOrder(oServ, pServ); break;
                    case "2": PerformBulkOrder(oServ, pServ); break;
                    case "3": DisplayProductTable(pServ.GetAllProducts()); WaitForKey(); break;
                }
            }
        }

        static void PerformPlaceOrder(IOrderService service, IProductService pService)
        {
            // UX FIX: Show IDs before asking for one
            DisplayProductTable(pService.GetAllProducts());

            int id = SafeReadInt("\nEnter Product ID (0 to cancel)");
            if (id <= 0) return;

            int qty = SafeReadInt("Enter Quantity");
            if (qty <= 0) { NotifyError("Quantity must be positive."); return; }

            service.PlaceOrder(loggedInUserId, id, qty);
            WaitForKey();
        }

        static void PerformBulkOrder(IOrderService service, IProductService pService)
        {
            Dictionary<int, int> cart = new Dictionary<int, int>();
            while (true)
            {
                DrawHeader("SHOPPING CART BUILDER");
                DisplayProductTable(pService.GetAllProducts());

                Console.WriteLine("\n--- YOUR CURRENT CART ---");
                if (cart.Count == 0) Console.WriteLine(" (Empty)");
                else foreach (var pair in cart) Console.WriteLine($" Item ID {pair.Key}: {pair.Value} units");

                int id = SafeReadInt("\nAdd Item ID (or 0 to Finish/Checkout)");
                if (id == 0) break;

                int q = SafeReadInt("Quantity");
                if (q > 0) cart[id] = q;
            }

            if (cart.Count > 0)
            {
                Console.WriteLine("\n[SYS] Processing bulk order through BLL...");
                service.PlaceOrder(loggedInUserId, cart);
            }
            WaitForKey();
        }

        static void ShowHistoryMenu(IOrderService oServ)
        {
            while (true)
            {
                DrawHeader("ORDER MANAGEMENT");
                Console.WriteLine(" 1. View My Order History");
                Console.WriteLine(" 2. Cancel an Order");
                Console.WriteLine(" 3. Back");

                string choice = Prompt("Selection");
                if (choice == "3") break;
                if (choice == "1") { ShowHistoryInternal(oServ); WaitForKey(); }
                else if (choice == "2") PerformCancelFlow(oServ);
            }
        }

        static void PerformCancelFlow(IOrderService service)
        {
            // UX FIX: Show orders first so user can actually see which ID to pick
            if (ShowHistoryInternal(service))
            {
                int oid = SafeReadInt("\nEnter Order ID to cancel (0 to go back)");
                if (oid > 0)
                {
                    Console.Write($"\nConfirm cancellation of Order #{oid}? (Type 'YES'): ");
                    if (Console.ReadLine()?.ToUpper() == "YES") service.CancelOrder(oid);
                    else NotifyError("Cancellation aborted.");
                }
            }
            WaitForKey();
        }

        static void ShowProductMgmt(IProductService service)
        {
            while (true)
            {
                DrawHeader("CATALOG MANAGEMENT");
                Console.WriteLine(" 1. Add New Catalog Item");
                Console.WriteLine(" 2. Modify Existing Item");
                Console.WriteLine(" 3. Remove Item");
                Console.WriteLine(" 4. Back");

                string choice = Prompt("Action");
                if (choice == "4") break;

                switch (choice)
                {
                    case "1":
                        Product p = new Product();
                        p.ProductName = Prompt("Product Name");
                        p.Price = SafeReadDecimal("Price");
                        p.StockQuantity = SafeReadInt("Initial Stock");
                        p.DiscountPercentage = SafeReadDecimal("Discount %");
                        service.AddProduct(p);
                        WaitForKey();
                        break;
                    case "2":
                        DisplayProductTable(service.GetAllProducts()); // UX FIX
                        int id = SafeReadInt("\nEnter ID to edit (0 to cancel)");
                        Product exist = service.GetProductById(id);
                        if (exist != null)
                        {
                            exist.ProductName = PromptDefault("New Name", exist.ProductName);
                            exist.Price = SafeReadDecimalDefault("New Price", exist.Price);
                            service.UpdateProduct(id, exist);
                        }
                        else NotifyError("Product not found.");
                        WaitForKey();
                        break;
                    case "3":
                        DisplayProductTable(service.GetAllProducts()); // UX FIX
                        service.DeleteProduct(SafeReadInt("\nEnter ID to delete"));
                        WaitForKey();
                        break;
                }
            }
        }

        static void ShowInventoryMgmt(IInventoryService service)
        {
            while (true)
            {
                DrawHeader("INVENTORY CONTROL");
                Console.WriteLine(" 1. View Stock Levels / Low Stock Audit");
                Console.WriteLine(" 2. Perform Restock Operation");
                Console.WriteLine(" 3. Back");

                string choice = Prompt("Tool");
                if (choice == "3") break;

                if (choice == "1")
                {
                    int thresh = SafeReadInt("Alert Threshold (Default 10)");
                    DisplayInventoryTable(service.GetLowStockItems(thresh == 0 ? 10 : thresh));
                    WaitForKey();
                }
                else if (choice == "2")
                {
                    // UX FIX: Show current stock status before asking for ID
                    DisplayInventoryTable(service.GetLowStockItems(9999));
                    int id = SafeReadInt("\nItem ID to restock (0 to cancel)");
                    if (id > 0)
                    {
                        int qty = SafeReadInt("Units to add");
                        service.RestockItem(id, qty);
                    }
                    WaitForKey();
                }
            }
        }
        #endregion

        #region UI Toolkit & Input Helpers
        static void DrawHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine($"   {title}");
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }

        static string Prompt(string label) { Console.Write($"\n{label} > "); return Console.ReadLine() ?? ""; }

        static string PromptDefault(string label, string def)
        {
            Console.Write($"{label} [{def}] > ");
            string input = Console.ReadLine();
            return string.IsNullOrEmpty(input) ? def : input;
        }

        static void NotifyError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] {msg}");
            Console.ResetColor();
            System.Threading.Thread.Sleep(1000);
        }

        static void WaitForKey() { Console.WriteLine("\nPress any key to continue..."); Console.ReadKey(); }

        static int SafeReadInt(string label)
        {
            Console.Write($"{label} : ");
            return int.TryParse(Console.ReadLine(), out int r) ? r : 0;
        }

        static decimal SafeReadDecimal(string label)
        {
            Console.Write($"{label} : ");
            return decimal.TryParse(Console.ReadLine(), out decimal r) ? r : 0;
        }

        static decimal SafeReadDecimalDefault(string label, decimal def)
        {
            Console.Write($"{label} [{def}] : ");
            string input = Console.ReadLine();
            return decimal.TryParse(input, out decimal r) ? r : def;
        }

        static void DisplayProductTable(List<Product> list)
        {
            if (list == null || list.Count == 0) { Console.WriteLine(" (Menu is currently empty)"); return; }
            Console.WriteLine("\n ID | Name                 | Price   | Stock");
            Console.WriteLine(" -------------------------------------------");
            foreach (var p in list.OrderBy(x => x.ProductId))
                Console.WriteLine($" {p.ProductId,-2} | {p.ProductName,-20} | ${p.Price,-7:0.00} | {p.StockQuantity}");
        }

        static void DisplayInventoryTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) { Console.WriteLine(" (No matching items found)"); return; }
            Console.WriteLine("\n ID | Product Name         | Current Stock");
            Console.WriteLine(" -------------------------------------------");
            foreach (DataRow r in dt.Rows)
                Console.WriteLine($" {r["ItemID"],-2} | {r["Name"],-20} | {r["StockQuantity"]}");
        }

        static bool ShowHistoryInternal(IOrderService s)
        {
            DataTable dt = s.GetUserOrderHistory(loggedInUserId);
            if (dt == null || dt.Rows.Count == 0) { Console.WriteLine(" (No orders found in your history)"); return false; }
            Console.WriteLine("\n Order# | Date     | Product            | Total");
            Console.WriteLine(" ----------------------------------------------");
            foreach (DataRow r in dt.Rows)
                Console.WriteLine($" {r["OrderID"],-6} | {Convert.ToDateTime(r["OrderDate"]):MM/dd} | {r["ProductName"],-18} | ${r["SubTotal"]:0.00}");
            return true;
        }

        static bool PerformLogin(IUserService s)
        {
            DrawHeader("ACCOUNT LOGIN");
            string e = Prompt("Email"), p = Prompt("Password");
            int uid = s.Login(e, p);
            if (uid > 0)
            {
                loggedInUserId = uid;
                loggedInUserName = e.Contains("@") ? e.Split('@')[0] : e;
                return true;
            }
            NotifyError("Login Failed. Verify your credentials.");
            return false;
        }

        static void PerformRegistration(IUserService s)
        {
            DrawHeader("CREATE NEW ACCOUNT");
            if (s.Register(Prompt("Full Name"), Prompt("Email Address"), Prompt("Password")))
                Console.WriteLine("\n[SUCCESS] Welcome! Your account is ready. Please log in.");
            WaitForKey();
        }
        #endregion
    }
}