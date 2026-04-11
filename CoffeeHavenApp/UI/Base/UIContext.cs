using CoffeeHavenDB.Interfaces;
using CoffeeHavenApp.Models;

namespace CoffeeHavenApp.UI.Base
{
    public class UIContext
    {
        public IUserService UserService { get; }
        public IProductService ProductService { get; }
        public IOrderService OrderService { get; }
        public IInventoryService InventoryService { get; }
        public Session Session { get; }

        public UIContext(
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IInventoryService inventoryService,
            Session session)
        {
            UserService = userService;
            ProductService = productService;
            OrderService = orderService;
            InventoryService = inventoryService;
            Session = session;
        }
    }
}
