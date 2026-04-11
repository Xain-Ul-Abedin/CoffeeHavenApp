using CoffeeHavenDB.Models;

namespace CoffeeHavenApp
{
    public class ShoppingCart
    {
            private List<Product> _items = new List<Product>();
            public void Add(Product p) => _items.Add(p);
            public void Remove(int id) => _items.RemoveAll(x => x.ProductId == id);

            public bool RemoveOne(int id)
            {
                var item = _items.FirstOrDefault(x => x.ProductId == id);
                if (item != null)
                {
                    _items.Remove(item);
                    return true;
                }
                return false;
            }

            public void UpdateQuantity(int id, int newQty)
            {
                var existing = _items.Where(x => x.ProductId == id).ToList();
                _items.RemoveAll(x => x.ProductId == id);
                if (existing.Count > 0)
                {
                    var prototype = existing[0];
                    for (int i = 0; i < newQty; i++) _items.Add(prototype);
                }
            }

            public List<Product> GetItems() => _items;
            public decimal GetTotal() => _items.Sum(x => x.Price);
            public void Clear() => _items.Clear();
       
    }
}