using System;
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
            string[] data = content.Split(new string[] { ",[PD]," }, StringSplitOptions.None);
            for (int i = 0; i < data.Length - 1; ++i)
            {
                ShopItem item = new ShopItem(data[i].Split(','));
                if (item.Id > 0)
                {
                    _items.Add(item);
                }
            }
            Id = int.Parse(data[data.Length - 1]);
        }
    }
}
