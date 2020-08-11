using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PROProtocol
{
    public class ItemsDatabase
    {
        public class ItemInfo
        {
            public string Name;
            public string Desc;
        }

        private static ItemsDatabase _instance;
        public static ItemsDatabase Instance => _instance ?? (_instance = new ItemsDatabase());

        private static Dictionary<int, ItemInfo> _items;

        private ItemsDatabase()
        {
            _items = ResourcesUtil.GetResource<Dictionary<int, ItemInfo>>("Items.json");
        }

        public ItemInfo Get(int itemId)
        {
            if (_items.TryGetValue(itemId, out ItemInfo item))
            {
                return item;
            }
            return null;
        }

        public string GetItemName(int itemId)
        {
            if (_items.TryGetValue(itemId, out ItemInfo item))
            {
                return item.Name;
            }
            return null;
        }
    }
}
