using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Csml {

    public class Cache<T> where T : Cache<T>, new() {

        private static readonly CacheConfigAttribute Config = typeof(T).GetCustomAttribute<CacheConfigAttribute>();

        private const string JsonDataFileName = "cache.json";

        public string Hash;

        [JsonIgnore]
        public string Directory => Path.Combine(Config.GetRootDirectory(), Hash);

        public Uri GetFileUri(string fileName) => new Uri(Config.GetRootUri(), $"{Hash}/{fileName}");

        public static T Create(string hash) {
            var result = new T() { Hash = hash };

            Utils.CreateDirectory(result.Directory);

            return result;
        }

        public static T Load(string hash) {
            var path = Path.Combine(Config.GetRootDirectory(), hash, JsonDataFileName);

            return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) : null;
        }

        public void Save() {
            Utils.CreateDirectory(Directory);

            var jsonFullPath = Path.Combine(Directory, JsonDataFileName);
            var jsonData = JsonConvert.SerializeObject((T)this);

            File.WriteAllText(jsonFullPath, jsonData);
        }
    }
}