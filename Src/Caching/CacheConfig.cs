using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Csml {

    public static class CacheConfig {
        public static string PublicCacheDirectory { get; set; }

        public static Uri PublicCacheUri { get; set; }

        public static string PrivateCacheDirectory { get; set; }

        static CacheConfig() {

        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class CacheConfigAttribute : Attribute { 
        public string CacheName { get; private set; }
        public bool IsPublicCache { get; private set; }

        public CacheConfigAttribute(string cacheName, bool isPublicCache) {
            CacheName = cacheName;
            IsPublicCache = isPublicCache;
        }

        public string GetRootDirectory() { 
            return Path.Combine(IsPublicCache ? CacheConfig.PublicCacheDirectory : CacheConfig.PrivateCacheDirectory, CacheName);
        }

        public Uri GetRootUri() {
            if (IsPublicCache && CacheConfig.PublicCacheUri != null) {
                return new Uri(CacheConfig.PublicCacheUri, CacheName + "/");     
            }

            return new Uri(GetRootDirectory() + "/");
        }
    }


}
