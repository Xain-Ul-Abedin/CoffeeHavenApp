using CoffeeHaven.DAL;
using CoffeeHaven.Interfaces;
using CoffeeHaven.Models;
using CoffeeHaven.Services;
using CoffeeHavenApp.Helpers;
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
    /// Coffee Haven Console Application
    /// Role-Based Access Control with Admin/Customer workflows.
    /// </summary>
    class App
    {
        // ── Role-Based Access Control ──────────────────────────────
        private const string AdminSecretKey = "ADMIN2026";

        private static int loggedInUserId = -1;
        private static string loggedInUserName = string.Empty;
        private static string loggedInUserEmail = string.Empty;
        private static string loggedInUserRole = string.Empty;   // "Admin" or "Customer"
        private static ShoppingCart cart = new ShoppingCart();

        private static bool IsAdmin => loggedInUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            // DAL
            IProductRepository productRepo = new ProductDAL();
            IOrderRepository orderRepo = new OrderDAL();
            IUserRepository userRepo = new UserDAL();
            IInventoryRepository inventoryRepo = new InventoryDAL();

            // BLL / Services
            IProductService productService = new ProductService(productRepo);
            IOrderService orderService = new OrderService(orderRepo, productService);
            IUserService userService = new UserService(userRepo);
            IInventoryService inventoryService = new InventoryService(inventoryRepo);

            Console.Title = "Coffee Haven";

            ShowLoginMenu(userService, productService, orderService, inventoryService);
        }

        // ============================================================
        // LOGIN / MAIN MENUS
        // ============================================================

        static void ShowLoginMenu(
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IInventoryService inventoryService)
        {
            while (true)
            {
                DrawHeader("WELCOME TO COFFEE HAVEN");
                Console.WriteLine(" 1. Login");
                Console.WriteLine(" 2. Register");
                Console.WriteLine(" 3. View Accounts List (Debug)");
                Console.WriteLine(" 4. Exit");

                string choice = Prompt("Selection").ToUpperInvariant();

                switch (choice)
                {
                    case "1":
                        if (PerformLogin(userService))
                        {
                            ShowMainMenu(productService, orderService, inventoryService, userService);
                        }
                        break;

                    case "2":
                        PerformRegistration(userService);
                        break;

                    case "3":
                        ShowDebugAccountsList(userService);
                        break;

                    case "4":
                        Environment.Exit(0);
                        return;

                    default:
                        ErrorMessage("Invalid selection. Please choose 1, 2, 3, or 4.");
                        Pause();
                        break;
                }
            }
        }

        static void ShowMainMenu(
            IProductService productService,
            IOrderService orderService,
            IInventoryService inventoryService,
            IUserService userService)
        {
            if (IsAdmin)
                ShowAdminDashboard(productService, orderService, inventoryService, userService);
            else
                ShowCustomerDashboard(productService, orderService, userService);
        }

        // ── ADMIN DASHBOARD ───────────────────────────────────────
        static void ShowAdminDashboard(
            IProductService productService,
            IOrderService orderService,
            IInventoryService inventoryService,
            IUserService userService)
        {
            while (true)
            {
                DrawHeader($"ADMIN DASHBOARD | {loggedInUserName.ToUpperInvariant()}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  Role: Administrator");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(" 1. Order Management");
                Console.WriteLine(" 2. Product Management");
                Console.WriteLine(" 3. Inventory Management");
                Console.WriteLine(" 4. Customer Management");
                Console.WriteLine(" 5. Settings");
                Console.WriteLine(" 6. Logout");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        ShowOrderManagementMenu(productService, orderService);
                        break;

                    case "2":
                        ShowProductManagementMenu(productService);
                        break;

                    case "3":
                        ShowInventoryManagementMenu(inventoryService);
                        break;

                    case "4":
                        ShowCustomerManagementMenu(userService);
                        break;

                    case "5":
                        bool accountDeleted = ShowProfileMenu(userService);
                        if (accountDeleted) return; // account deleted — exit to welcome screen
                        break;

                    case "6":
                        if (ConfirmLogout())
                            return;
                        break;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        // ── CUSTOMER DASHBOARD ────────────────────────────────────
        static void ShowCustomerDashboard(
            IProductService productService,
            IOrderService orderService,
            IUserService userService)
        {
            while (true)
            {
                DrawHeader($"WELCOME, {loggedInUserName.ToUpperInvariant()}!");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("  Role: Customer");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(" 1. Shop");
                Console.WriteLine(" 2. My Profile");
                Console.WriteLine(" 3. Logout");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        ShowOrderManagementMenu(productService, orderService);
                        break;

                    case "2":
                        bool accountDeleted = ShowProfileMenu(userService);
                        if (accountDeleted)
                            return; // account deleted — exit to welcome screen
                        break;

                    case "3":
                        if (ConfirmLogout())
                            return;
                        break;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        // ── LOGOUT (with confirmation) ────────────────────────────
        static bool ConfirmLogout()
        {
            if (Confirm("\nAre you sure you want to logout? (y/n): "))
            {
                loggedInUserId = -1;
                loggedInUserName = string.Empty;
                loggedInUserEmail = string.Empty;
                loggedInUserRole = string.Empty;
                cart.Clear();
                SuccessMessage("Logged out successfully.");
                Pause();
                return true;
            }
            return false;
        }

        // ── PROFILE MANAGEMENT ────────────────────────────────────
        /// <summary>
        /// Returns true if the account was deleted (caller must exit to welcome screen).
        /// </summary>
        static bool ShowProfileMenu(IUserService userService)
        {
            while (true)
            {
                DrawHeader("MY PROFILE");
                Console.WriteLine(" 1. View Profile");
                Console.WriteLine(" 2. Update Name");
                Console.WriteLine(" 3. Update Email");
                Console.WriteLine(" 4. Change Password");
                Console.WriteLine(" 5. Delete Account");
                Console.WriteLine(" 6. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        ViewProfile(userService);
                        Pause();
                        break;

                    case "2":
                        UpdateNameFlow(userService);
                        break;

                    case "3":
                        UpdateEmailFlow(userService);
                        break;

                    case "4":
                        ChangePasswordFlow(userService);
                        break;

                    case "5":
                        if (DeleteAccountFlow(userService))
                            return true; // account deleted
                        break;

                    case "6":
                        return false;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void ViewProfile(IUserService userService)
        {
            DrawHeader("PROFILE INFO");

            string fullName = userService.GetUserFullName(loggedInUserId);
            string email = userService.GetUserEmail(loggedInUserId);
            int points = userService.GetPoints(loggedInUserId);

            Console.WriteLine();
            Console.WriteLine($"  Name           : {fullName}");
            Console.WriteLine($"  Email          : {email}");
            Console.WriteLine($"  Role           : {loggedInUserRole}");
            Console.WriteLine($"  Loyalty Points : {points}");
        }

        static void UpdateNameFlow(IUserService userService)
        {
            DrawHeader("UPDATE NAME");

            string currentName = userService.GetUserFullName(loggedInUserId);
            Console.WriteLine($"\n  Current Name: {currentName}");

            string newName = Prompt("Enter new name");
            if (string.IsNullOrWhiteSpace(newName))
            {
                InfoMessage("No changes made.");
                Pause();
                return;
            }

            if (userService.UpdateFullName(loggedInUserId, newName))
            {
                loggedInUserName = newName;
                SuccessMessage("Name updated successfully.");
            }
            else
            {
                ErrorMessage("Failed to update name.");
            }

            Pause();
        }

        static void UpdateEmailFlow(IUserService userService)
        {
            DrawHeader("UPDATE EMAIL");

            string currentEmail = userService.GetUserEmail(loggedInUserId);
            Console.WriteLine($"\n  Current Email: {currentEmail}");

            string newEmail = Prompt("Enter new email");
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                InfoMessage("No changes made.");
                Pause();
                return;
            }

            if (userService.UpdateUserEmail(loggedInUserId, newEmail))
            {
                loggedInUserEmail = newEmail;
                SuccessMessage("Email updated successfully.");
            }
            else
            {
                ErrorMessage("Failed to update email.");
            }

            Pause();
        }

        static void ChangePasswordFlow(IUserService userService)
        {
            DrawHeader("CHANGE PASSWORD");

            string currentPassword = Prompt("Enter current password");

            // Verify current password first
            if (!userService.VerifyPassword(loggedInUserId, currentPassword))
            {
                ErrorMessage("Current password is incorrect.");
                Pause();
                return;
            }

            string newPassword = Prompt("Enter new password (min 6 characters)");
            string confirmPassword = Prompt("Confirm new password");

            if (newPassword != confirmPassword)
            {
                ErrorMessage("Passwords do not match.");
                Pause();
                return;
            }

            if (userService.ChangePassword(loggedInUserId, currentPassword, newPassword))
                SuccessMessage("Password changed successfully.");
            else
                ErrorMessage("Password change failed. Check requirements.");

            Pause();
        }

        /// <summary>
        /// Returns true if the account was successfully deleted.
        /// </summary>
        static bool DeleteAccountFlow(IUserService userService)
        {
            DrawHeader("DELETE ACCOUNT");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  WARNING: This action is permanent and cannot be undone.");
            Console.WriteLine("  All your account data will be removed.");
            Console.ResetColor();

            Console.Write("\n  Type DELETE to confirm: ");
            string confirm = Console.ReadLine();

            if (confirm == null || !confirm.Trim().Equals("DELETE", StringComparison.Ordinal))
            {
                InfoMessage("Account deletion cancelled.");
                Pause();
                return false;
            }

            string password = Prompt("Enter your password to confirm");

            if (userService.DeleteAccount(loggedInUserId, password))
            {
                SuccessMessage("Account deleted. You will be returned to the welcome screen.");
                Pause();

                // Clear all session state
                loggedInUserId = -1;
                loggedInUserName = string.Empty;
                loggedInUserEmail = string.Empty;
                loggedInUserRole = string.Empty;
                cart.Clear();

                return true;
            }
            else
            {
                ErrorMessage("Account deletion failed. Password may be incorrect.");
                Pause();
                return false;
            }
        }

        // ============================================================
        // ORDER MANAGEMENT
        // ============================================================

        static void ShowOrderManagementMenu(IProductService productService, IOrderService orderService)
        {
            while (true)
            {
                DrawHeader("ORDER MANAGEMENT");
                Console.WriteLine(" 1. Browse Menu");
                Console.WriteLine(" 2. Search Products");
                Console.WriteLine(" 3. Single Item Checkout");
                Console.WriteLine(" 4. Cart & Integrated Payment Checkout");
                Console.WriteLine(" 5. View Order History");
                Console.WriteLine(" 6. Search Order History");
                Console.WriteLine(" 7. Cancel Order");
                Console.WriteLine(" 8. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplayProductTable(productService.GetAllProducts());
                        Pause();
                        break;

                    case "2":
                        SearchProductsFlow(productService);
                        break;

                    case "3":
                        HandleSingleItemCheckout(productService, orderService);
                        break;

                    case "4":
                        HandleCartCheckout(productService, orderService);
                        break;

                    case "5":
                        ShowOrderHistory(orderService);
                        Pause();
                        break;

                    case "6":
                        SearchOrderHistoryFlow(orderService);
                        break;

                    case "7":
                        PerformCancelFlow(orderService);
                        break;

                    case "8":
                        return;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void HandleSingleItemCheckout(IProductService productService, IOrderService orderService)
        {
            DrawHeader("SINGLE ITEM CHECKOUT");

            List<Product> products = productService.GetAllProducts();
            DisplayProductTable(products);

            int productId = SafeReadInt("\nEnter Product ID");
            if (productId <= 0)
            {
                ErrorMessage("Invalid Product ID.");
                Pause();
                return;
            }

            int quantity = SafeReadInt("Enter Quantity");
            if (quantity <= 0)
            {
                ErrorMessage("Quantity must be greater than zero.");
                Pause();
                return;
            }

            Product product = productService.GetProductById(productId);
            if (product == null)
            {
                ErrorMessage("Product not found.");
                Pause();
                return;
            }

            if (!productService.IsProductAvailable(productId, quantity))
            {
                ErrorMessage($"Only {product.StockQuantity} unit(s) available for {product.ProductName}.");
                Pause();
                return;
            }

            decimal total = product.Price * quantity;

            Console.WriteLine("\nOrder Summary");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Item     : {product.ProductName}");
            Console.WriteLine($"Quantity : {quantity}");
            Console.WriteLine($"Unit Price: Rs. {product.Price:0.00}");
            Console.WriteLine($"Total    : Rs. {total:0.00}");

            if (!Confirm("\nProceed to payment and place order? (y/n): "))
            {
                InfoMessage("Checkout cancelled.");
                Pause();
                return;
            }

            if (!ProcessPayment(total))
            {
                ErrorMessage("Payment failed. Order was not placed.");
                Pause();
                return;
            }

            try
            {
                orderService.PlaceOrder(loggedInUserId, productId, quantity);
                SuccessMessage("Order placed successfully.");
            }
            catch (Exception ex)
            {
                ErrorMessage($"Order placement failed: {ex.Message}");
            }

            Pause();
        }

        static void HandleCartCheckout(IProductService productService, IOrderService orderService)
        {
            while (true)
            {
                DrawHeader("SHOPPING CART");
                DisplayCart(productService);

                Console.WriteLine("\n 1. Add Item");
                Console.WriteLine(" 2. Remove Item");
                Console.WriteLine(" 3. Proceed to Payment");
                Console.WriteLine(" 4. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        AddItemToCart(productService);
                        break;

                    case "2":
                        RemoveItemFromCart();
                        break;

                    case "3":
                        CheckoutCart(productService, orderService);
                        return;

                    case "4":
                        return;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void AddItemToCart(IProductService productService)
        {
            DrawHeader("ADD ITEM TO CART");
            DisplayProductTable(productService.GetAllProducts());

            int productId = SafeReadInt("\nEnter Product ID");
            if (productId <= 0)
            {
                ErrorMessage("Invalid Product ID.");
                Pause();
                return;
            }

            Product product = productService.GetProductById(productId);
            if (product == null)
            {
                ErrorMessage("Product not found.");
                Pause();
                return;
            }

            int quantity = SafeReadInt("Enter Quantity");
            if (quantity <= 0)
            {
                ErrorMessage("Quantity must be greater than zero.");
                Pause();
                return;
            }

            if (!productService.IsProductAvailable(productId, quantity))
            {
                ErrorMessage($"Only {product.StockQuantity} unit(s) available for {product.ProductName}.");
                Pause();
                return;
            }

            for (int i = 0; i < quantity; i++)
            {
                cart.Add(product);
            }

            SuccessMessage("Item added to cart.");
            Pause();
        }

        static void RemoveItemFromCart()
        {
            DrawHeader("REMOVE ITEM FROM CART");
            DisplayCartRaw();

            int productId = SafeReadInt("\nEnter Product ID to remove");
            if (productId <= 0)
            {
                ErrorMessage("Invalid Product ID.");
                Pause();
                return;
            }

            cart.Remove(productId);
            SuccessMessage("Item removed from cart.");
            Pause();
        }

        static void CheckoutCart(IProductService productService, IOrderService orderService)
        {
            DrawHeader("CART CHECKOUT");

            List<Product> items = cart.GetItems();
            if (items.Count == 0)
            {
                ErrorMessage("Cart is empty.");
                Pause();
                return;
            }

            Dictionary<int, int> orderMap = items
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Count());

            DisplayCart(productService);

            decimal total = cart.GetTotal();

            Console.WriteLine($"\nGrand Total: Rs. {total:0.00}");

            if (!Confirm("\nProceed to payment and place order? (y/n): "))
            {
                InfoMessage("Checkout cancelled.");
                Pause();
                return;
            }

            // Pre-check availability before payment for better UX
            foreach (KeyValuePair<int, int> entry in orderMap)
            {
                if (!productService.IsProductAvailable(entry.Key, entry.Value))
                {
                    Product p = productService.GetProductById(entry.Key);
                    string name = p != null ? p.ProductName : $"Item #{entry.Key}";
                    int available = p != null ? p.StockQuantity : 0;

                    ErrorMessage($"{name} has only {available} unit(s) available.");
                    Pause();
                    return;
                }
            }

            if (!ProcessPayment(total))
            {
                ErrorMessage("Payment failed. Order was not placed.");
                Pause();
                return;
            }

            try
            {
                orderService.PlaceOrder(loggedInUserId, orderMap);
                cart.Clear();
                SuccessMessage("Payment successful. Order placed.");
            }
            catch (Exception ex)
            {
                ErrorMessage($"Order placement failed: {ex.Message}");
            }

            Pause();
        }

        static bool ProcessPayment(decimal amount)
        {
            DrawHeader("PAYMENT");
            Console.WriteLine($"Amount Due: Rs. {amount:0.00}");
            Console.WriteLine("\nSelect Payment Method:");
            Console.WriteLine(" 1. Credit Card");
            Console.WriteLine(" 2. PayPal");

            string choice = Prompt("Payment Method");

            Payment payment = new Payment
            {
                Amount = amount
            };

            IPaymentGateway gateway = null;

            if (choice == "1")
            {
                payment.PaymentMethod = "Credit Card";
                payment.CardNumber = Prompt("Card Number");
                payment.ExpirationDate = Prompt("Expiration Date (MM/YY)");
                payment.CVV = Prompt("CVV");
                gateway = new CreditCardPaymentGateway();
            }
            else if (choice == "2")
            {
                payment.PaymentMethod = "PayPal";
                payment.PayPalEmail = Prompt("PayPal Email");
                gateway = new PayPalPaymentGateway();
            }
            else
            {
                ErrorMessage("Invalid payment method.");
                return false;
            }

            try
            {
                PaymentService paymentService = new PaymentService(gateway);
                return paymentService.ExecutePayment(payment);
            }
            catch (Exception ex)
            {
                ErrorMessage($"Payment error: {ex.Message}");
                return false;
            }
        }

        static void ShowOrderHistory(IOrderService orderService)
        {
            DrawHeader("ORDER HISTORY");

            DataTable dt = orderService.GetUserOrderHistory(loggedInUserId);
            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No orders found.");
                return;
            }

            Console.WriteLine("\n Order# | Date       | Product             | Qty | Unit Price | SubTotal");
            Console.WriteLine(" ------------------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                int orderId = Convert.ToInt32(row["OrderID"]);
                DateTime orderDate = Convert.ToDateTime(row["OrderDate"]);
                string productName = row["ProductName"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);
                decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                decimal subTotal = Convert.ToDecimal(row["SubTotal"]);

                Console.WriteLine(
                    $" {orderId,-6} | {orderDate:MM/dd/yyyy} | {productName,-19} | {quantity,-3} | Rs. {unitPrice,8:0.00} | Rs. {subTotal,8:0.00}");
            }
        }

        static void PerformCancelFlow(IOrderService orderService)
        {
            DrawHeader("CANCEL ORDER");

            ShowOrderHistory(orderService);

            int orderId = SafeReadInt("\nEnter Order ID to cancel (0 to go back)");
            if (orderId <= 0)
            {
                InfoMessage("Cancellation cancelled.");
                Pause();
                return;
            }

            Console.Write($"Confirm cancellation of Order #{orderId}? Type YES to proceed: ");
            string confirm = Console.ReadLine();

            if (confirm == null || !confirm.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                InfoMessage("Cancellation aborted.");
                Pause();
                return;
            }

            try
            {
                orderService.CancelOrder(orderId);
                SuccessMessage("Order cancelled successfully.");
            }
            catch (Exception ex)
            {
                ErrorMessage($"Cancellation failed: {ex.Message}");
            }

            Pause();
        }

        // ============================================================
        // PRODUCT MANAGEMENT
        // ============================================================

        static void ShowProductManagementMenu(IProductService productService)
        {
            while (true)
            {
                DrawHeader("PRODUCT MANAGEMENT");
                Console.WriteLine(" 1. View Products");
                Console.WriteLine(" 2. Search Products");
                Console.WriteLine(" 3. Add Product");
                Console.WriteLine(" 4. Update Product");
                Console.WriteLine(" 5. Delete Product");
                Console.WriteLine(" 6. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplayProductTable(productService.GetAllProducts());
                        Pause();
                        break;

                    case "2":
                        SearchProductsFlow(productService);
                        break;

                    case "3":
                        AddProductFlow(productService);
                        break;

                    case "4":
                        UpdateProductFlow(productService);
                        break;

                    case "5":
                        DeleteProductFlow(productService);
                        break;

                    case "6":
                        return;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void AddProductFlow(IProductService productService)
        {
            DrawHeader("ADD PRODUCT");

            Product product = new Product();
            product.ProductName = Prompt("Product Name");
            product.Description = Prompt("Description");
            product.Price = SafeReadDecimal("Price");
            product.StockQuantity = SafeReadInt("Initial Stock");
            product.DiscountPercentage = SafeReadDecimal("Discount Percentage");
            product.CategoryId = SafeReadInt("Category ID");

            productService.AddProduct(product);
            Pause();
        }

        static void UpdateProductFlow(IProductService productService)
        {
            DrawHeader("UPDATE PRODUCT");
            DisplayProductTable(productService.GetAllProducts());

            int productId = SafeReadInt("\nEnter Product ID to update");
            if (productId <= 0)
            {
                ErrorMessage("Invalid Product ID.");
                Pause();
                return;
            }

            Product existing = productService.GetProductById(productId);
            if (existing == null)
            {
                ErrorMessage("Product not found.");
                Pause();
                return;
            }

            existing.ProductName = PromptDefault("Product Name", existing.ProductName);
            existing.Description = PromptDefault("Description", existing.Description);
            existing.Price = SafeReadDecimalDefault("Price", existing.Price);
            existing.StockQuantity = SafeReadIntDefault("Stock Quantity", existing.StockQuantity);
            existing.DiscountPercentage = SafeReadDecimalDefault("Discount Percentage", existing.DiscountPercentage);
            existing.CategoryId = SafeReadIntDefault("Category ID", existing.CategoryId);

            productService.UpdateProduct(productId, existing);
            Pause();
        }

        static void DeleteProductFlow(IProductService productService)
        {
            DrawHeader("DELETE PRODUCT");
            DisplayProductTable(productService.GetAllProducts());

            int productId = SafeReadInt("\nEnter Product ID to delete");
            if (productId <= 0)
            {
                ErrorMessage("Invalid Product ID.");
                Pause();
                return;
            }

            Console.Write($"Delete product #{productId}? Type YES to confirm: ");
            string confirm = Console.ReadLine();

            if (confirm == null || !confirm.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                InfoMessage("Deletion cancelled.");
                Pause();
                return;
            }

            productService.DeleteProduct(productId);
            Pause();
        }

        // ============================================================
        // INVENTORY MANAGEMENT
        // ============================================================

        static void ShowInventoryManagementMenu(IInventoryService inventoryService)
        {
            while (true)
            {
                DrawHeader("INVENTORY MANAGEMENT");
                Console.WriteLine(" 1. View Low Stock Items");
                Console.WriteLine(" 2. Search Inventory");
                Console.WriteLine(" 3. Restock Item");
                Console.WriteLine(" 4. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        int threshold = SafeReadInt("Low stock threshold");
                        if (threshold <= 0) threshold = 10;
                        DisplayInventoryTable(inventoryService.GetLowStockItems(threshold));
                        Pause();
                        break;

                    case "2":
                        SearchInventoryFlow(inventoryService);
                        break;

                    case "3":
                        DisplayInventoryTable(inventoryService.GetLowStockItems(999999));
                        int itemId = SafeReadInt("\nEnter Item ID to restock");
                        if (itemId <= 0)
                        {
                            ErrorMessage("Invalid Item ID.");
                            Pause();
                            break;
                        }

                        int qty = SafeReadInt("Enter quantity to add");
                        if (qty <= 0)
                        {
                            ErrorMessage("Quantity must be greater than zero.");
                            Pause();
                            break;
                        }

                        inventoryService.RestockItem(itemId, qty);
                        SuccessMessage("Restock operation completed.");
                        Pause();
                        break;

                    case "4":
                        return;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        // ============================================================
        // CUSTOMER MANAGEMENT (Admin Only)
        // ============================================================

        static void ShowCustomerManagementMenu(IUserService userService)
        {
            while (true)
            {
                DrawHeader("CUSTOMER MANAGEMENT");
                Console.WriteLine(" 1. View All Users");
                Console.WriteLine(" 2. Search / Filter Users");
                Console.WriteLine(" 3. Add User");
                Console.WriteLine(" 4. Update User");
                Console.WriteLine(" 5. Delete User");
                Console.WriteLine(" 6. Back");

                string choice = Prompt("Selection");

                switch (choice)
                {
                    case "1":
                        DisplayUserTable(userService.GetAllUsers());
                        Pause();
                        break;

                    case "2":
                        SearchUsersFlow(userService);
                        break;

                    case "3":
                        AdminAddUserFlow(userService);
                        break;

                    case "4":
                        AdminUpdateUserFlow(userService);
                        break;

                    case "5":
                        AdminDeleteUserFlow(userService);
                        break;

                    case "6":
                        return;

                    default:
                        ErrorMessage("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void DisplayUserTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No users found.");
                return;
            }

            Console.WriteLine("\n ID  | Name                 | Email                  | Role     | Points");
            Console.WriteLine(" --------------------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                string name = row["FullName"].ToString();
                string email = row["Email"].ToString();
                string role = row["Role"].ToString();
                int points = row["LoyaltyPoints"] == DBNull.Value ? 0 : Convert.ToInt32(row["LoyaltyPoints"]);

                Console.WriteLine(
                    $" {userId,-3} | {name,-20} | {email,-22} | {role,-8} | {points}");
            }
        }

        static void SearchUsersFlow(IUserService userService)
        {
            DrawHeader("SEARCH / FILTER USERS");

            DataTable dt = userService.GetAllUsers();
            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No users found to search.");
                Pause();
                return;
            }

            Console.WriteLine(" Filter by:");
            Console.WriteLine(" 1. Keyword (name, email, role)");
            Console.WriteLine(" 2. Role (Admin / Customer)");

            string filterChoice = Prompt("Selection");

            DataTable filtered;

            switch (filterChoice)
            {
                case "1":
                    string keyword = Prompt("Enter search keyword");
                    filtered = SearchHelper.SearchDataTable(dt, keyword);
                    break;

                case "2":
                    Console.WriteLine("\n 1. Admin");
                    Console.WriteLine(" 2. Customer");
                    string roleChoice = Prompt("Selection");
                    string roleFilter = roleChoice == "1" ? "Admin" : "Customer";
                    filtered = SearchHelper.FilterDataTableByColumn(dt, "Role", roleFilter);
                    break;

                default:
                    ErrorMessage("Invalid selection.");
                    Pause();
                    return;
            }

            if (filtered.Rows.Count == 0)
            {
                InfoMessage("No users matched the search criteria.");
                Pause();
                return;
            }

            SuccessMessage($"{filtered.Rows.Count} user(s) found:");
            DisplayUserTable(filtered);
            Pause();
        }

        static void AdminAddUserFlow(IUserService userService)
        {
            DrawHeader("ADD USER");

            Console.WriteLine(" Role for new user:");
            Console.WriteLine(" 1. Admin");
            Console.WriteLine(" 2. Customer");

            string roleChoice = Prompt("Selection");
            string role;

            switch (roleChoice)
            {
                case "1":
                    role = "Admin";
                    break;
                case "2":
                    role = "Customer";
                    break;
                default:
                    ErrorMessage("Invalid selection.");
                    Pause();
                    return;
            }

            string fullName = Prompt("Full Name");
            string email = Prompt("Email");
            string password = Prompt("Password");

            if (userService.Register(fullName, email, password, role))
                SuccessMessage($"{role} account created successfully.");
            else
                ErrorMessage("Failed to create user.");

            Pause();
        }

        static void AdminUpdateUserFlow(IUserService userService)
        {
            DrawHeader("UPDATE USER");
            DisplayUserTable(userService.GetAllUsers());

            int userId = SafeReadInt("\nEnter User ID to update");
            if (userId <= 0)
            {
                ErrorMessage("Invalid User ID.");
                Pause();
                return;
            }

            // Verify user exists
            string existingName = userService.GetUserFullName(userId);
            if (string.IsNullOrEmpty(existingName))
            {
                ErrorMessage("User not found.");
                Pause();
                return;
            }

            string existingEmail = userService.GetUserEmail(userId);
            string existingRole = userService.GetUserRole(userId);

            Console.WriteLine($"\n  Current Info:");
            Console.WriteLine($"    Name  : {existingName}");
            Console.WriteLine($"    Email : {existingEmail}");
            Console.WriteLine($"    Role  : {existingRole}");

            Console.WriteLine("\n  What to update?");
            Console.WriteLine("  1. Name");
            Console.WriteLine("  2. Email");
            Console.WriteLine("  3. Role");
            Console.WriteLine("  4. Cancel");

            string updateChoice = Prompt("Selection");

            switch (updateChoice)
            {
                case "1":
                    string newName = Prompt("New Name");
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        InfoMessage("No changes made.");
                        break;
                    }
                    if (userService.UpdateFullName(userId, newName))
                        SuccessMessage("Name updated.");
                    else
                        ErrorMessage("Failed to update name.");
                    break;

                case "2":
                    string newEmail = Prompt("New Email");
                    if (string.IsNullOrWhiteSpace(newEmail))
                    {
                        InfoMessage("No changes made.");
                        break;
                    }
                    if (userService.UpdateUserEmail(userId, newEmail))
                        SuccessMessage("Email updated.");
                    else
                        ErrorMessage("Failed to update email.");
                    break;

                case "3":
                    Console.WriteLine("\n  1. Admin");
                    Console.WriteLine("  2. Customer");
                    string roleChoice = Prompt("New Role");
                    string newRole = roleChoice == "1" ? "Admin" : "Customer";
                    if (userService.UpdateUserRole(userId, newRole))
                        SuccessMessage($"Role updated to {newRole}.");
                    else
                        ErrorMessage("Failed to update role.");
                    break;

                case "4":
                    InfoMessage("Update cancelled.");
                    break;

                default:
                    ErrorMessage("Invalid selection.");
                    break;
            }

            Pause();
        }

        static void AdminDeleteUserFlow(IUserService userService)
        {
            DrawHeader("DELETE USER");
            DisplayUserTable(userService.GetAllUsers());

            int userId = SafeReadInt("\nEnter User ID to delete");
            if (userId <= 0)
            {
                ErrorMessage("Invalid User ID.");
                Pause();
                return;
            }

            // Prevent admin from deleting themselves
            if (userId == loggedInUserId)
            {
                ErrorMessage("You cannot delete your own account from here. Use Profile > Delete Account instead.");
                Pause();
                return;
            }

            string userName = userService.GetUserFullName(userId);
            if (string.IsNullOrEmpty(userName))
            {
                ErrorMessage("User not found.");
                Pause();
                return;
            }

            string userEmail = userService.GetUserEmail(userId);
            Console.WriteLine($"\n  About to delete: {userName} ({userEmail})");
            Console.Write("  Type YES to confirm: ");
            string confirm = Console.ReadLine();

            if (confirm == null || !confirm.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                InfoMessage("Deletion cancelled.");
                Pause();
                return;
            }

            if (userService.AdminDeleteUser(userId))
                SuccessMessage($"User #{userId} deleted successfully.");
            else
                ErrorMessage("Failed to delete user.");

            Pause();
        }

        static void ShowDebugAccountsList(IUserService userService)
        {
            DrawHeader("ACCOUNTS LIST (DEBUG)");
            DataTable dt = userService.GetDebugUserList();

            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No users found.");
                Pause();
                return;
            }

            Console.WriteLine("\n ID  | Role     | Email                  | Password");
            Console.WriteLine(" ---------------------------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                string role = row["Role"].ToString();
                string email = row["Email"].ToString();
                string pass = row["PasswordHash"] == DBNull.Value ? "(NULL)" : row["PasswordHash"].ToString();

                Console.WriteLine($" {userId,-3} | {role,-8} | {email,-22} | {pass}");
            }
            Pause();
        }

        // ============================================================
        // AUTH
        // ============================================================

        static bool PerformLogin(IUserService userService)
        {
            DrawHeader("LOGIN");

            // Step 1: Select role
            Console.WriteLine(" Login as:");
            Console.WriteLine(" 1. Admin");
            Console.WriteLine(" 2. Customer");

            string roleChoice = Prompt("Selection");
            string role;

            switch (roleChoice)
            {
                case "1":
                    role = "Admin";
                    break;
                case "2":
                    role = "Customer";
                    break;
                default:
                    ErrorMessage("Invalid selection.");
                    Pause();
                    return false;
            }

            // Step 2: Credentials
            string email = Prompt("Email");
            string password = Prompt("Password");

            // Step 3: Role-aware authentication
            int userId = userService.Login(email, password, role);
            if (userId > 0)
            {
                loggedInUserId = userId;
                loggedInUserEmail = email.Trim();
                loggedInUserRole = role;

                // Use full name from DB for display
                string fullName = userService.GetUserFullName(userId);
                loggedInUserName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName
                    : (email.Contains("@") ? email.Split('@')[0] : email);

                SuccessMessage($"Login successful.  Welcome, {loggedInUserName}!  [{loggedInUserRole}]");
                Pause();
                return true;
            }

            ErrorMessage($"Login failed. No {role} account found with those credentials.");
            Pause();
            return false;
        }

        static void PerformRegistration(IUserService userService)
        {
            DrawHeader("REGISTER");

            // Step 1: Select role
            Console.WriteLine(" Register as:");
            Console.WriteLine(" 1. Admin");
            Console.WriteLine(" 2. Customer");

            string roleChoice = Prompt("Selection");
            string role;

            switch (roleChoice)
            {
                case "1":
                    role = "Admin";
                    break;
                case "2":
                    role = "Customer";
                    break;
                default:
                    ErrorMessage("Invalid selection.");
                    Pause();
                    return;
            }

            // Step 2: Admin secret key gate
            if (role == "Admin")
            {
                string secretKey = Prompt("Enter Admin Secret Key");
                if (!secretKey.Equals(AdminSecretKey, StringComparison.Ordinal))
                {
                    ErrorMessage("Invalid secret key. Admin registration denied.");
                    Pause();
                    return;
                }
            }

            // Step 3: Collect user info
            string fullName = Prompt("Full Name");
            string email = Prompt("Email");
            string password = Prompt("Password");

            // Step 4: Register with role
            if (userService.Register(fullName, email, password, role))
                SuccessMessage($"{role} registration successful. You can now log in.");
            else
                ErrorMessage("Registration failed.");

            Pause();
        }

        // ============================================================
        // SEARCH FLOWS (powered by SearchHelper)
        // ============================================================

        static void SearchProductsFlow(IProductService productService)
        {
            DrawHeader("SEARCH PRODUCTS");

            string keyword = Prompt("Enter search keyword (name/description)");

            List<Product> results = SearchHelper.SearchProducts(
                productService.GetAllProducts(), keyword);

            if (results.Count == 0)
            {
                InfoMessage($"No products matched \"{keyword}\".");
                Pause();
                return;
            }

            SuccessMessage($"{results.Count} product(s) found:");

            // Offer sorting
            Console.WriteLine("\nSort by: 1. Name  2. Price  3. Stock  (Enter to skip)");
            string sortChoice = Prompt("Sort option");

            switch (sortChoice)
            {
                case "1":
                    results = SearchHelper.SortProducts(results, "name");
                    break;
                case "2":
                    results = SearchHelper.SortProducts(results, "price");
                    break;
                case "3":
                    results = SearchHelper.SortProducts(results, "stock");
                    break;
            }

            DisplayProductTable(results);
            Pause();
        }

        static void SearchOrderHistoryFlow(IOrderService orderService)
        {
            DrawHeader("SEARCH ORDER HISTORY");

            DataTable dt = orderService.GetUserOrderHistory(loggedInUserId);
            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No orders found to search.");
                Pause();
                return;
            }

            string keyword = Prompt("Enter search keyword (product name, date, etc.)");

            DataTable filtered = SearchHelper.SearchDataTable(dt, keyword);

            if (filtered.Rows.Count == 0)
            {
                InfoMessage($"No orders matched \"{keyword}\".");
                Pause();
                return;
            }

            SuccessMessage($"{filtered.Rows.Count} order(s) found:");

            Console.WriteLine("\n Order# | Date       | Product             | Qty | Unit Price | SubTotal");
            Console.WriteLine(" ------------------------------------------------------------------------");

            foreach (DataRow row in filtered.Rows)
            {
                int orderId = Convert.ToInt32(row["OrderID"]);
                DateTime orderDate = Convert.ToDateTime(row["OrderDate"]);
                string productName = row["ProductName"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);
                decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                decimal subTotal = Convert.ToDecimal(row["SubTotal"]);

                Console.WriteLine(
                    $" {orderId,-6} | {orderDate:MM/dd/yyyy} | {productName,-19} | {quantity,-3} | Rs. {unitPrice,8:0.00} | Rs. {subTotal,8:0.00}");
            }

            Pause();
        }

        static void SearchInventoryFlow(IInventoryService inventoryService)
        {
            DrawHeader("SEARCH INVENTORY");

            DataTable dt = inventoryService.GetLowStockItems(999999); // get all
            if (dt == null || dt.Rows.Count == 0)
            {
                InfoMessage("No inventory records found to search.");
                Pause();
                return;
            }

            string keyword = Prompt("Enter search keyword (product name, etc.)");

            DataTable filtered = SearchHelper.SearchDataTable(dt, keyword);

            if (filtered.Rows.Count == 0)
            {
                InfoMessage($"No inventory items matched \"{keyword}\".");
                Pause();
                return;
            }

            SuccessMessage($"{filtered.Rows.Count} item(s) found:");
            DisplayInventoryTable(filtered);
            Pause();
        }

        // ============================================================
        // DISPLAY HELPERS
        // ============================================================

        static void DisplayProductTable(List<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("No products available.");
                return;
            }

            Console.WriteLine("\n ID | Name                 | Price      | Stock");
            Console.WriteLine("--------------------------------------------------------");

            foreach (Product p in products.OrderBy(x => x.ProductId))
            {
                Console.WriteLine(
                    $" {p.ProductId,-2} | {p.ProductName,-20} | Rs. {p.Price,8:0.00} | {p.StockQuantity}");
            }
        }

        static void DisplayInventoryTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                Console.WriteLine("No matching inventory records found.");
                return;
            }

            Console.WriteLine("\n ID | Product Name         | Current Stock");
            Console.WriteLine("-------------------------------------------");

            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine(
                    $" {row["ItemID"],-2} | {row["Name"],-20} | {row["StockQuantity"]}");
            }
        }

        static void DisplayCart(IProductService productService)
        {
            List<Product> items = cart.GetItems();

            if (items.Count == 0)
            {
                Console.WriteLine("Cart is empty.");
                return;
            }

            var grouped = items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    UnitPrice = g.First().Price,
                    Quantity = g.Count(),
                    LineTotal = g.First().Price * g.Count()
                })
                .ToList();

            Console.WriteLine("\n ID | Product               | Qty | Unit Price | Line Total");
            Console.WriteLine("------------------------------------------------------------");

            foreach (var item in grouped)
            {
                Console.WriteLine(
                    $" {item.ProductId,-2} | {item.ProductName,-20} | {item.Quantity,-3} | Rs. {item.UnitPrice,8:0.00} | Rs. {item.LineTotal,9:0.00}");
            }

            Console.WriteLine($"\nCart Total: Rs. {cart.GetTotal():0.00}");
        }

        static void DisplayCartRaw()
        {
            List<Product> items = cart.GetItems();

            if (items.Count == 0)
            {
                Console.WriteLine("Cart is empty.");
                return;
            }

            var grouped = items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    Quantity = g.Count(),
                    UnitPrice = g.First().Price
                })
                .ToList();

            Console.WriteLine("\n ID | Product               | Qty | Unit Price");
            Console.WriteLine("------------------------------------------------");

            foreach (var item in grouped)
            {
                Console.WriteLine(
                    $" {item.ProductId,-2} | {item.ProductName,-20} | {item.Quantity,-3} | Rs. {item.UnitPrice,8:0.00}");
            }
        }

        // ============================================================
        // INPUT HELPERS
        // ============================================================

        static string Prompt(string label)
        {
            Console.Write($"\n{label} > ");
            string input = Console.ReadLine();
            return input == null ? string.Empty : input.Trim();
        }

        static string PromptDefault(string label, string defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] > ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            return input.Trim();
        }

        static int SafeReadInt(string label)
        {
            Console.Write($"{label} : ");
            int value;
            if (int.TryParse(Console.ReadLine(), out value))
                return value;

            return 0;
        }

        static int SafeReadIntDefault(string label, int defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] : ");
            string input = Console.ReadLine();
            int value;
            if (int.TryParse(input, out value))
                return value;

            return defaultValue;
        }

        static decimal SafeReadDecimal(string label)
        {
            Console.Write($"{label} : ");
            decimal value;
            if (decimal.TryParse(Console.ReadLine(), out value))
                return value;

            return 0m;
        }

        static decimal SafeReadDecimalDefault(string label, decimal defaultValue)
        {
            Console.Write($"{label} [{defaultValue}] : ");
            string input = Console.ReadLine();
            decimal value;
            if (decimal.TryParse(input, out value))
                return value;

            return defaultValue;
        }

        static bool Confirm(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();

            if (input == null)
                return false;

            input = input.Trim();
            return input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                   input.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        // ============================================================
        // UI MESSAGES
        // ============================================================

        static void DrawHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("   " + title);
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }

        static void SuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] " + message);
            Console.ResetColor();
        }

        static void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ERROR] " + message);
            Console.ResetColor();
        }

        static void InfoMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[INFO] " + message);
            Console.ResetColor();
        }
    }
}