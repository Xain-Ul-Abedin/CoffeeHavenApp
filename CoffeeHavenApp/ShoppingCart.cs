using CoffeeHavenDB.Models;

namespace CoffeeHavenApp
{
    internal class ShoppingCart
    {
            private List<Product> _items = new List<Product>();
            public void Add(Product p) => _items.Add(p);
            public void Remove(int id) => _items.RemoveAll(x => x.ProductId == id);
            public List<Product> GetItems() => _items;
            public decimal GetTotal() => _items.Sum(x => x.Price);
            public void Clear() => _items.Clear();
       
    }
}