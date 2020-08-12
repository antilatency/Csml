using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Htmlilka;

namespace Csml{

    [CacheConfig("Downloads", true)]
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
            var source =  string.Join("?", paths.Select(x => Path.GetRelativePath(CsmlApplication.ProjectRootDirectory, x)).OrderBy(x=>x));
            return Hash.CreateFromString(source).ToString();
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

            return cache.GetFileUri(cache.FileName).ToString()+"|"+ StringUtils.HumanizeSize(cache.FileSize);
        }

        public override Node Generate(Context context) {
            return new Tag("div")
                .Add(new Behaviour("Downloadable", PrimaryName, OptionsTree).Generate(context));
        }
    }
}
