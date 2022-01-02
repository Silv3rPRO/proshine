using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROProtocol
{
    public static class ResourcesUtil
    {
        public static StreamReader GetResource(string fileName)
        {
            var path = Path.Combine("Resources", fileName);
            if (File.Exists(path))
            {
                return new StreamReader(File.OpenRead(path));
            }
            throw new Exception($"Resource could not be found: {fileName}");
        }

        public static T GetResource<T>(string fileName)
        {
            using (var res = GetResource(fileName))
            {
                var serializer = new JsonSerializer();
                using (var jtr = new JsonTextReader(res))
                {
                    return serializer.Deserialize<T>(jtr);
                }
            }
        }
    }
}
