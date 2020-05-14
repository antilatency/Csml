using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Csml {
    class HashUtils {

        public static byte[] GetFileHash(string path, HashAlgorithm algo) {
            using (var fileStream = File.OpenRead(path)) {
                using (var bufferedStream = new BufferedStream(fileStream, 1000000)) {
                    return algo.ComputeHash(bufferedStream);
                }
            }
        }

        public static byte[] GetFileHash(string path) {
            using (var md5 = new MD5CryptoServiceProvider()) {
                return GetFileHash(path, md5);
            }
        }

        public static byte[] GetFilesHash(IEnumerable<string> paths) {
            using (var md5 = new MD5CryptoServiceProvider()) {
                var hashes = new List<byte>(10 * 32);

                foreach (var path in paths) {
                    hashes.AddRange(GetFileHash(path, md5));
                }

                return md5.ComputeHash(hashes.ToArray());
            }
        }

        public static byte[] GetDirectoryHash(string path, string searchPattern = "*") {
            return GetFilesHash(Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories));
        }

        public static string HashToString(byte[] hash) {
            var output = new StringBuilder();

            for (int i = 0; i < hash.Length; i++) {
                output.Append(hash[i].ToString("x2"));
            }
            return output.ToString();
        }
    }
}
