using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Csml {
    public class PngUtils {

        public static void NormalizeChunks(string fileName) {
            if(new FileInfo(fileName).Extension != ".png") { return; }
            var fileStream = new BufferedStream(File.OpenRead(fileName), 1200000);
            var header = ReadBytes(fileStream, 8);
            var chunks = GetChunksFromStream(fileStream);
            fileStream.Close();
            WritePng(new FileStream(fileName, FileMode.Create), chunks, header);
            return;
        }

        private static byte[] ReadBytes(Stream stream, int n) {
            var buffer = new byte[n];
            stream.Read(buffer, 0, n);
            return buffer;
        }

        private static List<Chunk> GetChunksFromStream(Stream stream) {
            var chunks = new List<Chunk>();
            while(stream.Position < stream.Length) {
                chunks.Add(ChunkFromStream(stream));
            }
            return chunks;
        }

        private static Chunk ChunkFromStream(Stream stream) {
            var length = ReadBytes(stream, 4);
            var name = ReadBytes(stream, 4);
            var data = ReadBytes(stream, Convert.ToInt32(BitConverter.ToUInt32(length.Reverse().ToArray(), 0)));
            var crc = ReadBytes(stream, 4);
            return new Chunk(length, name, data, crc);
        }

        private static void WritePng(Stream stream, List<Chunk> chunks, byte[] header) {
            WriteBytes(stream, header);
            chunks.Where(x => x.Name == "IHDR").FirstOrDefault()?.WriteToStream(stream);
            chunks.Where(x => x.Name == "PLTE").FirstOrDefault()?.WriteToStream(stream);
            foreach(var ancillaryChunk in chunks.Where(x => !x.IsChunkCritical && x.Name != "tIME").OrderBy(x => x.HashedData)) {
                ancillaryChunk.WriteToStream(stream);
            }
            foreach(var dataChunk in chunks.Where(x => x.Name == "IDAT")) {
                dataChunk.WriteToStream(stream);
            }
            chunks.Where(x => x.Name == "IEND").FirstOrDefault()?.WriteToStream(stream);
        }

        private static void WriteBytes(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

        private class Chunk {
            byte[] _lenght;
            byte[] _name;
            byte[] _data;
            byte[] _crc;

            public string Name;

            public int Length;

            public string HashedData;
            public Chunk(byte[] length, byte[] name, byte[] data, byte[] crc) {
                _lenght = length;
                _name = name;
                _data = data;
                _crc = crc;
                Name = Encoding.UTF8.GetString(_name);
                Length = data.Length;
                HashedData = HashData(_data);
            }

            private static string ToString(byte[] hash) {
                if(hash != null && hash.Length > 0) {
                    var output = new StringBuilder();
                    for(int i = 0; i < hash.Length; i++) {
                        output.Append(hash[i].ToString("X2"));
                    }
                    return output.ToString();
                }
                return "INVALID_HASH";
            }

            private string HashData(byte[] data) {
                using var md5Hash = MD5.Create();
                return ToString(md5Hash.ComputeHash(data));
            }

            public bool IsChunkCritical => Name == "IHDR" || Name == "PLTE" || Name == "IDAT" || Name == "IEND";

            public void WriteToStream(Stream stream) {
                WriteBytes(stream, _lenght);
                WriteBytes(stream, _name);
                WriteBytes(stream, _data);
                WriteBytes(stream, _crc);
            }

            public override string ToString() => Name;

        }
    }
}
