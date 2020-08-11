using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PROProtocol
{
    public class AbilityData
    {
        public string Name { get; set; }
        public string Desc { get; set; }
    }

    public class AbilitiesManager
    {
        private static AbilitiesManager _instance;

        public static AbilitiesManager Instance
        {
            get
            {
                return _instance ?? (_instance = new AbilitiesManager());
            }
        }

        public Dictionary<int, AbilityData> Abilities { get; }

        private AbilitiesManager()
        {
            Abilities = ResourcesUtil.GetResource<Dictionary<int, AbilityData>>("Abilities.json");
        }
    }
}
