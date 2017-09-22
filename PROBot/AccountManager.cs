using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PROBot
{
    public class AccountManager
    {
        public AccountManager(string saveDirectoryPath)
        {
            Accounts = new Dictionary<string, Account>();
            SavePath = saveDirectoryPath;
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            LoadAccountDirectory(saveDirectoryPath);
        }

        public string SavePath { get; set; }
        public Dictionary<string, Account> Accounts { get; set; }

        public void SaveAccount(string accountName)
        {
            if (!Accounts.ContainsKey(accountName))
                return;
            var account = Accounts[accountName];
            var json = JsonConvert.SerializeObject(account, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });
            if (account.FileName == null || account.FileName == "")
                account.FileName = accountName;
            foreach (var c in Path.GetInvalidFileNameChars())
                account.FileName = account.FileName.Replace(c.ToString(), "");
            var path = Path.Combine(SavePath, account.FileName + ".json");
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            File.WriteAllText(path, json);
        }

        public void DeleteAccount(string accountName)
        {
            if (Accounts.ContainsKey(accountName))
            {
                var account = Accounts[accountName];
                Accounts.Remove(accountName);
                var path = Path.Combine(SavePath, account.FileName + ".json");
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public void LoadAccountFile(string filePath)
        {
            var fileText = File.ReadAllText(filePath);
            JObject json;
            try
            {
                json = JsonConvert.DeserializeObject(fileText) as JObject;
            }
            catch (JsonReaderException)
            {
                return;
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            if (name == "" || name == null)
                return;

            var account = JsonConvert.DeserializeObject<Account>(json.ToString());
            if (string.IsNullOrEmpty(account.Name))
                return;
            account.FileName = Path.GetFileNameWithoutExtension(filePath);
            Accounts[account.FileName] = account;
        }

        public void LoadAccountDirectory(string directoryPath)
        {
            if (!Directory.Exists(SavePath))
                return;
            foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
                LoadAccountFile(file);
        }
    }
}