using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Csml {
    struct Hash {
        public byte[] Data { get; private set; }

        Hash(byte[] data) => Data = data;

        public override string ToString() {
            const string Invalid = "INVALID_HASH";
            var data = Data;
            if(data != null && data.Length > 0) {
                var output = new StringBuilder();
                for(int i = 0; i < data.Length; i++) {
                    output.Append(data[i].ToString("X2"));
                }
                return output.ToString();
            }
            return Invalid;
        }

        private static HashAlgorithm CreateHashAlgorithm() {
            return new MD5CryptoServiceProvider();
        }

        private static Hash CreateFromFile(string path, HashAlgorithm algo) {
            using var fileStream = File.OpenRead(path);
            using var bufferedStream = new BufferedStream(fileStream, 1000000);
            return new Hash(algo.ComputeHash(bufferedStream));
        }

        public static Hash CreateFromFile(string path) {
            using var algo = CreateHashAlgorithm();
            return CreateFromFile(path, algo);
        }

        public static Hash CreateFromFiles(IEnumerable<string> paths) {
            using var algo = CreateHashAlgorithm();
            var hashes = new List<byte>(10 * 32);

            foreach(var path in paths) {
                hashes.AddRange(CreateFromFile(path, algo).Data);
            }

            return new Hash(algo.ComputeHash(hashes.ToArray()));
        }

        public static Hash CreateFromFiles(string path, string searchPattern = "*") {
            return CreateFromFiles(Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories));
        }

        public static Hash CreateFromString(string text) {
            using var algo = CreateHashAlgorithm();
            var bytes = Encoding.Unicode.GetBytes(text);
            return new Hash(algo.ComputeHash(bytes));
        }
    }
}
