using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;

using HtmlAgilityPack;

namespace Csml
{
    public class DownloadableCache: Cache<DownloadableCache> {
    }

    public sealed class Downloadable : Element<Downloadable> {

        private string SourcePath { get; set; }
        private string DownloadableFileName { get; set; }
        private string Tooltip { get; set; }

        private DownloadableCache _cache = null;

        public Downloadable(string sourcePath, string tooltip = "") {
            SourcePath = ConvertPathToAbsolute(sourcePath);

            if (!IsValidSource(SourcePath)) {
                Log.Error.Here($"Invalid downloadable source = {SourcePath}");
            }

            DownloadableFileName = GetDownloadableFileNameFromSource(SourcePath);
            Tooltip = tooltip;
        }

        private string GetDownloadableFileNameFromSource(string sourcePath) {
            var name = IsDirectory(sourcePath) ? new DirectoryInfo(sourcePath).Name : Path.GetFileName(sourcePath);

            return IsNeedToBeArchived(sourcePath) ? name + ".zip" : name;
        }

        private bool IsValidSource(string sourcePath) {
            return Directory.Exists(sourcePath) || File.Exists(sourcePath);
        }

        private bool IsDirectory(string sourcePath) {
            return File.GetAttributes(sourcePath).HasFlag(FileAttributes.Directory);
        }

        private bool IsNeedToBeArchived(string sourcePath) {
            return IsDirectory(sourcePath);
        }

        private string CalculateHash(string sourcePath) {
            var md5 = new MD5CryptoServiceProvider();
            Func<string, byte[]> getFileHash = (filePath) => {
                using (var fileStream = File.OpenRead(filePath)) {
                    using (var bufferedStream = new BufferedStream(fileStream, 1000000)) {
                        return md5.ComputeHash(bufferedStream);
                    }
                }
            };

            Func<byte[], string> binToString = (data) => {
                var hashString = new StringBuilder();
                for (int i = 0; i < data.Length; i++) {
                    hashString.Append(data[i].ToString("x2"));
                }
                return hashString.ToString();
            };

            if (IsDirectory(sourcePath)) {
                var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                var hashes = new List<byte>(files.Length * 32);
                files.ForEach(x => hashes.AddRange(getFileHash(x)));

                return binToString(md5.ComputeHash(hashes.ToArray()));
            } else {
                return binToString(getFileHash(sourcePath));
            }
        }

        private Uri GetContent() {
            if (_cache == null) {
                var hash = CalculateHash(SourcePath);

                _cache = DownloadableCache.Load(hash);

                if (_cache == null) {
                    _cache = DownloadableCache.Create(hash);

                    var outputPath = Path.Combine(_cache.Directory, DownloadableFileName);

                    if (IsNeedToBeArchived(SourcePath)) {
                        ZipFile.CreateFromDirectory(SourcePath, outputPath);
                    } else {
                        File.Copy(SourcePath, outputPath);
                    }

                    _cache.Save();
                }
            }

            return _cache.GetFileUri(DownloadableFileName);
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var content = GetContent();
            var button = HtmlNode.CreateNode("<a>");

            button.AddClass("Downloadable");
            button.SetAttributeValue("href", content.ToString());
            button.InnerHtml = DownloadableFileName;

            if (!string.IsNullOrEmpty(Tooltip)) {
                button.SetAttributeValue("title", Tooltip);
            }

            yield return button;
        }

    }
}
