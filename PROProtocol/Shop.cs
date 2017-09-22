using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PROProtocol
{
    public class Shop
    {
        private readonly List<ShopItem> _items = new List<ShopItem>();

        public Shop(string content)
        {
            var data = content.Split(',');
            if (data.Length >= 31)
            {
                for (var i = 0; i < 10; ++i)
                {
                    var item = new ShopItem(data, i * 3);
                    if (item.Id > 0)
                        _items.Add(item);
                }
                Id = int.Parse(data[30]);
            }
        }

        public int Id { get; }

        public ReadOnlyCollection<ShopItem> Items => _items.AsReadOnly();
    }
}