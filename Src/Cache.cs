using Newtonsoft.Json;
using System;
using System.IO;

namespace Csml {
    public class Cache {
        public string Hash;
        [JsonIgnore]
        public string Directory => Path.Combine(RootDirectory, Hash);
        public static string RootDirectory;
        protected Cache() { }
    }

    public class Cache<T> : Cache where T : Cache<T>, new() {
        

        public static T Create(string hash) {
            if (string.IsNullOrEmpty(RootDirectory))
                throw new Exception("Cache.RootDirectory undefined.");
            
            var result = new T();
            result.Hash = hash;
            Utils.CreateDirectory(result.Directory);

            return result;
        }

        public static T Load(string hash) {
            if (string.IsNullOrEmpty(RootDirectory)) return null;
            var directory = Path.Combine(RootDirectory, hash);
            var jsonFullPath = Path.Combine(directory, "cache.json");
            if (!File.Exists(jsonFullPath)) return null;
            var json = File.ReadAllText(jsonFullPath);
            var result = JsonConvert.DeserializeObject<T>(json);

            return result;
        }
        public bool CanSave() {
            return !string.IsNullOrEmpty(RootDirectory);
        }
        //public bool Stored => !string.IsNullOrEmpty(Directory);

        public void Save() {
            if (string.IsNullOrEmpty(RootDirectory))
                throw new Exception("Cache.RootDirectory undefined.");

            //var directory = Path.Combine(RootDirectory, hash);
            Utils.CreateDirectory(Directory);
            var jsonFullPath = Path.Combine(Directory, "cache.json");
            var json = JsonConvert.SerializeObject((T)this);
            File.WriteAllText(jsonFullPath, json);
            //Directory = directory;
        }
    }
}