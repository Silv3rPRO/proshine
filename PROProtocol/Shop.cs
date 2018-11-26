using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PROProtocol
{
    public class Shop
    {
        public int Id { get; private set; }

        private List<ShopItem> _items = new List<ShopItem>();
        public ReadOnlyCollection<ShopItem> Items { get { return _items.AsReadOnly(); } }

        public Shop(string content)
        {
            string[] data = content.Split(new[] { "[PD]," }, StringSplitOptions.None);
            if (data.Length >= 4)
            {
                for (int i = 0; i < data.Length - 1; ++i)
                {
                    var itemData = data[i].Split(',');
                    var item = new ShopItem(itemData, 0);
                    if (item.Id > 0)
                        _items.Add(item);
                }
                Id = int.Parse(data.LastOrDefault());
            }
        }
    }
}
