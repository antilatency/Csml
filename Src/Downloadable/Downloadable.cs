using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;

using HtmlAgilityPack;
using System.Linq;

namespace Csml{

    public class DownloadableCache : Cache<DownloadableCache> {
        /// <summary>
        /// Size in bytes of cached file
        /// </summary>
        public long FileSize;
        public string FileName;
    }



    public sealed class Downloadable : Element<Downloadable> {
        public enum PathFragment {
            Directory = 0,
            Version = 1,
            CanSelectAll = 1<<1
        }

        string PrimaryName { get; set; }
        string SourcePath { get; set; }
        readonly object OptionsTree;

        public Downloadable(string primaryName, string sourcePath, params PathFragment[] pathFragments) {
            SourcePath = ConvertPathToAbsolute(sourcePath);

            PrimaryName = primaryName;
            if (PrimaryName == null) {
                PrimaryName = Path.GetFileNameWithoutExtension(sourcePath);
            }
            if (File.Exists(SourcePath)) {
                OptionsTree = FinalizeOptionsBranch(SourcePath, PrimaryName);
            } else {
                OptionsTree = BuildOptionsTree(SourcePath, pathFragments, 0, PrimaryName);
            }            
        }

        public object BuildOptionsTree(string path, PathFragment[] pathFragments, int index, string name) {            

            if (pathFragments.Length == index) return FinalizeOptionsBranch(path, name);

            var subDirectories = Directory.GetDirectories(path);
            if (subDirectories.Length == 1) {
                var subDirectoryName = Path.GetFileName(subDirectories[0]);
                return BuildOptionsTree(subDirectories[0], pathFragments, index + 1, name+ "_" + subDirectoryName);
            }
            var result = new Dictionary<string, object>();
            if (pathFragments[index].HasFlag(PathFragment.CanSelectAll)) {
                result.Add("All", FinalizeOptionsBranch(path, name));
            }

            if (pathFragments[index].HasFlag(PathFragment.Version)) {

                try {
                    var versions = subDirectories.ToDictionary(x => new SemVer(Path.GetFileName(x)))
                    .OrderByDescending(x => x.Key).ToList();
                    if (versions.Count == 0) return null;
                    var stable = versions.FirstOrDefault(x => x.Key.labelsString == "");
                    var latest = versions.First();
                    if (latest.Key != stable.Key) {
                        if (versions.Remove(stable)) {
                            versions.Insert(0, stable);
                        }
                    }

                    foreach (var v in versions) {
                        var e = BuildOptionsTree(v.Value, pathFragments, index + 1, name + "_" + v.Key);
                        var optionName = v.Key.ToString();
                        if (v.Key == stable.Key) {
                            optionName = "Stable " + optionName;
                        }
                        if (v.Key == latest.Key) {
                            optionName = "Latest " + optionName;
                        }
                        if (e != null)
                            result.Add(optionName, e);
                    }
                }
                catch (ArgumentException e) {
                    Log.Error.OnObject(this, e.Message);
                }

            } else {
                foreach (var d in subDirectories) {
                    var subDirectoryName = Path.GetFileName(d);
                    var e = BuildOptionsTree(d, pathFragments, index + 1, name + "_" + subDirectoryName);
                    if (e != null)
                        result.Add(subDirectoryName, e);
                }
            }            

            if (result.Count == 0) return null;
            return result;

        }


        public string CalculateHash(IEnumerable<string> paths) {            
            var source =  string.Join("?", paths.Select(x => Path.GetRelativePath(Application.ProjectRootDirectory, x)).OrderBy(x=>x));
            return Utils.ToHashString(source);
        }

        public string FinalizeOptionsBranch(string path, string name) {
            string[] files;

            if (File.Exists(path)) {
                files = new[] { path };
            } else {
                if (!Directory.Exists(path)) return null;
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToArray();
            }

            if (files.Length == 0) return null;

            var hash = CalculateHash(files);
            DownloadableCache cache = DownloadableCache.Load(hash);

            string fileName;
            if (files.Length == 1) {
                var extension = Path.GetExtension(files[0]);
                fileName = name + extension;
            } else {
                fileName = name + ".zip";
            }

            if (cache == null) {
                cache = DownloadableCache.Create(hash);
                cache.FileName = fileName;
                var outputPath = Path.Combine(cache.Directory, cache.FileName);

                if (files.Length == 1) {
                    File.Copy(files[0], outputPath);
                } else {
                    using var zip = ZipFile.Open(outputPath, ZipArchiveMode.Create);
                    foreach (var f in files) {
                        zip.CreateEntryFromFile(f, Path.GetRelativePath(path, f), CompressionLevel.Optimal);
                    }
                }
                cache.FileSize = new FileInfo(outputPath).Length;
                cache.Save();
            } else {
                if (fileName != cache.FileName) {
                    Log.Error.OnObject(this, $"Same content available with different names <{fileName}> and <{cache.FileName}>");
                }
            }


            /*string[] relativePaths = files.Select(x => Path.GetRelativePath(Application.ProjectRootDirectory, x)).ToArray();

            var sortedPaths = relativePaths.OrderBy(x => x);
            var commonPrefix = new string(
                sortedPaths.First().Substring(0, sortedPaths.Min(s => s.Length))
                .TakeWhile((c, i) => sortedPaths.All(s => s[i] == c)).ToArray()
                );


            


            DownloadableCache cache = DownloadableCache*/


            return cache.GetFileUri(cache.FileName).ToString()+"|"+ StringUtils.HumanizeSize(cache.FileSize);
        }





        public override IEnumerable<HtmlNode> Generate(Context context) {

            yield return HtmlNode.CreateNode("<div>").Do(x => {
                x.Add(new Behaviour("Downloadable",PrimaryName,OptionsTree).Generate(context));
            
            });

            /*var content = GetContent();
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

            yield return button;*/
        }


    }




        /*

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

        }*/
    }
