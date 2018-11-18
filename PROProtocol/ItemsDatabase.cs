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
            try
            {
                if (File.Exists("Resources/Items.json"))
                {
                    string json = File.ReadAllText("Resources/Items.json");
                    _items = JsonConvert.DeserializeObject<Dictionary<int, ItemInfo>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not read the items: " + ex.Message);
            }
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
