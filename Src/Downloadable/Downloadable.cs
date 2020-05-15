using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;

using HtmlAgilityPack;
using System.Linq;

namespace Csml
{
    public class DownloadableCache : Cache<DownloadableCache> {
        /// <summary>
        /// Size in bytes of cached file
        /// </summary>
        public long Size;
    }

    public enum  DownloadableIcon {
        None,

        Windows,
        Android, 
        Linux,

        Zip,
        Cloud
    }

    public sealed class Downloadable : Element<Downloadable> {

        public static readonly CompressionLevel ZipCompressionLevel = CompressionLevel.Optimal;

        private string SourcePath { get; set; }
        private string DownloadableFileName { get; set; }
        private DownloadableIcon Icon { get; set; }
        private string Tooltip { get; set; }
        private bool Zip { get; set; }
        private List<string> ZipEntries { get; set; }

        private DownloadableCache _cache = null;

        public Downloadable(string sourcePath, DownloadableIcon icon = DownloadableIcon.None, string tooltip = "", Func<string, bool> packToArchivePredicate = null) {
            SourcePath = ConvertPathToAbsolute(sourcePath);

            if (!IsValidSource(SourcePath)) {
                Log.Error.Here($"Invalid downloadable source = {SourcePath}");
            }

            Zip = IsNeedToBeArchived(SourcePath);
            DownloadableFileName = GetDownloadableFileNameFromSource(SourcePath, Zip);
            Icon = icon;
            Tooltip = tooltip;
            ZipEntries = null;

            if (Zip) {
                if (IsDirectory(SourcePath)) {
                    var entries = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);

                    if (packToArchivePredicate != null) {
                        ZipEntries = new List<string>(entries.Where(packToArchivePredicate));
                    } else {
                        ZipEntries = new List<string>(entries);
                    }
                } else {
                    ZipEntries = new List<string>(new string[] { SourcePath });
                }
            } 
        }

        private string GetDownloadableFileNameFromSource(string sourcePath, bool zip) {
            var name = IsDirectory(sourcePath) ? new DirectoryInfo(sourcePath).Name : Path.GetFileName(sourcePath);

            return zip ? name + ".zip" : name;
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
            if (Zip && ZipEntries != null && ZipEntries.Count > 0) {
                return HashUtils.HashToString(HashUtils.GetFilesHash(ZipEntries));
            } else {
                return HashUtils.HashToString(HashUtils.GetFileHash(sourcePath));
            }
        }

        private Uri GetContent() {
            if (_cache == null) {
                var hash = CalculateHash(SourcePath);

                _cache = DownloadableCache.Load(hash);

                if (_cache == null) {
                    _cache = DownloadableCache.Create(hash);

                    var outputPath = Path.Combine(_cache.Directory, DownloadableFileName);

                    if (Zip) {
                        using (var zip = ZipFile.Open(outputPath, ZipArchiveMode.Create)) {
                            foreach (var path in ZipEntries) {
                                zip.CreateEntryFromFile(path, Path.GetRelativePath(SourcePath, path), ZipCompressionLevel);
                            }
                        }
                    } else {
                        File.Copy(SourcePath, outputPath);
                    }

                    _cache.Size = new FileInfo(outputPath).Length;
                    _cache.Save();
                }
            }

            return _cache.GetFileUri(DownloadableFileName);
        }

        private string GetIconCssClass(DownloadableIcon icon) {
            if (icon != DownloadableIcon.None) {
                return string.Format("DownloadableIcon{0}", icon.ToString());
            }

            return string.Empty;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var content = GetContent();
            var button = HtmlNode.CreateNode("<a>");

            button.AddClass("Downloadable");
            button.SetAttributeValue("href", content.ToString());
            button.SetAttributeValue("download", DownloadableFileName);
            button.InnerHtml = DownloadableFileName;

            if (!string.IsNullOrEmpty(Tooltip)) {
                button.SetAttributeValue("title", Tooltip);
            } else {
                button.SetAttributeValue("title", string.Format("Download -- {0}", StringUtils.HumanizeSize(_cache.Size)));
            }

            if (Icon != DownloadableIcon.None) {
                var icon = HtmlNode.CreateNode("<i>");

                icon.AddClass("DownloadableIcon");
                icon.AddClass(GetIconCssClass(Icon));

                button.AppendChild(icon);
            }

            yield return button;
        }

    }
}
