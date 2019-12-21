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
        private const string AbilitiesFile = "Resources/Abilities.json";

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
            try
            {
                if (File.Exists(AbilitiesFile))
                {
                    string json = File.ReadAllText(AbilitiesFile);
                    Abilities = JsonConvert.DeserializeObject<Dictionary<int, AbilityData>>(json);
                }
                else
                {
                    Console.Error.WriteLine($"File '{AbilitiesFile}' is missing");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not read the abilities: " + ex.Message);
                return;
            }
        }
    }
}
