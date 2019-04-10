using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PROProtocol
{
    public class Shop
    {
        public int Id { get; private set; }

        private List<ShopItem> _items = new List<ShopItem>();
        public ReadOnlyCollection<ShopItem> Items { get { return _items.AsReadOnly(); } }

        public Shop(string content)
        {
            string[] data = content.Split(',');
            if (data.Length >= 41)
            {
                for (int i = 0; i < 10; ++i)
                {
                    ShopItem item = new ShopItem(data, i * 4);
                    if (item.Id > 0)
                    {
                        _items.Add(item);
                    }
                }
                Id = int.Parse(data[40]);
            }
        }
    }
}
