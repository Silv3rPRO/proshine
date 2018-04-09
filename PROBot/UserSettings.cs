using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PROBot
{
    public static class UserSettings
    {
        private static SettingsCache _settings;

        public static bool AutoReconnect
        {
            get { return _settings.AutoReconnect; }
            set
            {
                if (_settings.AutoReconnect != value)
                {
                    _settings.AutoReconnect = value;
                    _settings.Save();
                }
            }
        }

        public static bool AvoidStaff
        {
            get { return _settings.AvoidStaff; }
            set
            {
                if (_settings.AvoidStaff != value)
                {
                    _settings.AvoidStaff = value;
                    _settings.Save();
                }
            }
        }

        public static bool AutoEvolve
        {
            get { return _settings.AutoEvolve; }
            set
            {
                if (_settings.AutoEvolve != value)
                {
                    _settings.AutoEvolve = value;
                    _settings.Save();
                }
            }
        }

        public static string LastScript
        {
            get { return _settings.LastScript; }
            set
            {
                if (_settings.LastScript != value)
                {
                    _settings.LastScript = value;
                    _settings.Save();
                }
            }
        }

        static UserSettings()
        {
            try
            {
                if (File.Exists("Settings.json"))
                {
                    string fileText = File.ReadAllText("Settings.json");
                    JObject json = JsonConvert.DeserializeObject(fileText) as JObject;
                    _settings = JsonConvert.DeserializeObject<SettingsCache>(json.ToString());
                    return;
                }
            }
            catch { }
            _settings = new SettingsCache();
        }

        private class SettingsCache
        {
            public bool AutoReconnect;
            public bool AvoidStaff;
            public bool AutoEvolve = true;
            public string LastScript;

            public void Save()
            {
                string json = JsonConvert.SerializeObject(this, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                });
                File.WriteAllText("Settings.json", json);
            }
        }
    }
}
