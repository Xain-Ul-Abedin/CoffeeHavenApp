using System;
using CoffeeHavenApp.Models;

namespace CoffeeHavenApp.Models
{
    public class Session
    {
        public int UserId { get; set; } = -1;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;   // "Admin" or "Customer"
        public ShoppingCart Cart { get; set; } = new ShoppingCart();

        public bool IsAdmin => UserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public void Clear()
        {
            UserId = -1;
            UserName = string.Empty;
            UserEmail = string.Empty;
            UserRole = string.Empty;
            Cart.Clear();
        }
    }
}
