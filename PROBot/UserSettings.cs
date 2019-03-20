using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PROBot
{
    public class UserSettings
    {
        private SettingsCache _settings;

        public bool AutoReconnect
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

        public bool AvoidStaff
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

        public bool AutoEvolve
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

        public string LastScript
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

        public List<Sound> Sounds
        {
            get { return _settings.Sounds; }
            set
            {
                if (_settings.Sounds != value)
                {
                    _settings.Sounds = value;
                    _settings.Save();
                }
            }
        }

        public UserSettings()
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

            public List<Sound> Sounds = new List<Sound>
            {
                // Battle
                new Sound("CaptureFail", false, "capture_fail.wav"),
                new Sound("Captured", true, "captured.wav"),
                new Sound("CaptureAttempt", false, "capture_attempt.wav"),
                new Sound("Escaped", false, "escaped.wav"),
                new Sound("LevelUp", false, "levelup.wav"),
                new Sound("ShinyEncounter", true, "shiny.wav"),
                // Questing
                new Sound("Select", false, "select.wav"),
                new Sound("HiddenItemFound", true, "item_hidden.wav"),
                new Sound("ItemFound", false, "item.wav"),
                new Sound("PcUsage", false, "pc_turningon.wav"),
                new Sound("Purchase", true, "purchase.wav"),
                // Proshine
                new Sound("Pause", true, "pause.wav"),
                new Sound("LogOut", true, "logout.wav")
            };

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
