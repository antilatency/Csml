using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Htmlilka;

namespace Csml {

    [CacheConfig("Downloads", true)]
    public class DownloadableCache : Cache<DownloadableCache> {
        /// <summary>
        /// Size in bytes of cached file
        /// </summary>
        public long FileSize;
        public string FileName;
    }


    public class SearchSequencer {

        public int SettingsDepth = 0;

        [Flags]
        public enum SeqrchSequence : byte {
            Nothing = 0,
            Version = 1,
            Directory = 1 << 1,
            AllDirectories = 1 << 2,
            SpecifiedDirectory = 1 << 3
        }

        private Queue<SeqrchSequence> Options = new Queue<SeqrchSequence>();

        public SeqrchSequence GetNextOption() => Options.Dequeue();

        public SearchSequencer Version {
            get {
                SettingsDepth++;
                Options.Enqueue(SeqrchSequence.Version);
                return this;
            }
        }

        public SearchSequencer AnyDirectory {
            get {
                SettingsDepth++;
                Options.Enqueue(SeqrchSequence.Directory);
                return this;
            }
        }

        public SearchSequencer AllDirectories {
            get {
                SettingsDepth++;
                Options.Enqueue(SeqrchSequence.AllDirectories);
                return this;
            }
        }
    }

    public sealed class Downloadable : Element<Downloadable> {
        //public enum PathFragment {
        //    Directory = 0,
        //    Version = 1,
        //    CanSelectAll = 1 << 1
        //}

        string PrimaryName { get; set; }
        string SourcePath { get; set; }
        readonly object OptionsTree;
        //TODO search Sequencer!!!!
        public Downloadable(string primaryName, string sourcePath, SearchSequencer searchSequence = null) {
            SourcePath = ConvertPathToAbsolute(sourcePath);

            PrimaryName = primaryName;
            if(PrimaryName == null) {
                PrimaryName = Path.GetFileNameWithoutExtension(sourcePath);
            }
            if(File.Exists(SourcePath)) {
                OptionsTree = FinalizeOptionsBranch(SourcePath, PrimaryName);
            } else {
                OptionsTree = BuildOptionsTree(SourcePath, searchSequence, 0, PrimaryName);
            }
        }

        public object BuildOptionsTree(string path, SearchSequencer searchSequence, int index, string name) {

            if(searchSequence == null || searchSequence.SettingsDepth == index) { return FinalizeOptionsBranch(path, name); }
            var subDirectories = Directory.GetDirectories(path);
            if(subDirectories.Length == 1) {
                var subDirectoryName = Path.GetFileName(subDirectories[0]);
                return BuildOptionsTree(subDirectories[0], searchSequence, index + 1, name + "_" + subDirectoryName);
            }
            var result = new Dictionary<string, object>();
            switch(searchSequence.GetNextOption()) {
                case SearchSequencer.SeqrchSequence.AllDirectories:
                    result.Add("All", FinalizeOptionsBranch(path, name));
                    break;
                case SearchSequencer.SeqrchSequence.Version: {
                        try {
                            var versions = subDirectories.ToDictionary(x => new SemVer(Path.GetFileName(x)))
                            .OrderByDescending(x => x.Key).ToList();
                            if(versions.Count == 0) return null;
                            var stable = versions.FirstOrDefault(x => x.Key.labelsString == "");
                            var latest = versions.First();
                            if(latest.Key != stable.Key) {
                                if(versions.Remove(stable)) {
                                    versions.Insert(0, stable);
                                }
                            }

                            foreach(var v in versions) {
                                var e = BuildOptionsTree(v.Value, searchSequence, index + 1, name + "_" + v.Key);
                                var optionName = v.Key.ToString();
                                if(v.Key == stable.Key) {
                                    optionName = "Stable " + optionName;
                                }
                                if(v.Key == latest.Key) {
                                    optionName = "Latest " + optionName;
                                }
                                if(e != null)
                                    result.Add(optionName, e);
                            }
                        } catch(ArgumentException e) {
                            Log.Error.OnObject(this, e.Message);
                        }

                        break;
                    }

                default: {
                        foreach(var d in subDirectories) {
                            var subDirectoryName = Path.GetFileName(d);
                            var e = BuildOptionsTree(d, searchSequence, index + 1, name + "_" + subDirectoryName);
                            if(e != null)
                                result.Add(subDirectoryName, e);
                        }

                        break;
                    }
            }
            if(result.Count == 0) return null;
            return result;

        }

        public string CalculateHash(IEnumerable<string> paths) {
            var source = string.Join("?", paths.Select(x => Path.GetRelativePath(CsmlApplication.ProjectRootDirectory, x)).OrderBy(x => x));
            return Hash.CreateFromString(source).ToString();
        }

        public string FinalizeOptionsBranch(string path, string name) {
            string[] files;

            if(File.Exists(path)) {
                files = new[] { path };
            } else {
                if(!Directory.Exists(path)) return null;
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToArray();
            }

            if(files.Length == 0) return null;

            var hash = CalculateHash(files);
            DownloadableCache cache = DownloadableCache.Load(hash);

            string fileName;
            if(files.Length == 1) {
                var extension = Path.GetExtension(files[0]);
                fileName = name + extension;
            } else {
                fileName = name + ".zip";
            }

            if(cache == null) {
                cache = DownloadableCache.Create(hash);
                cache.FileName = fileName;
                var outputPath = Path.Combine(cache.Directory, cache.FileName);

                if(files.Length == 1) {
                    File.Copy(files[0], outputPath);
                } else {
                    using var zip = ZipFile.Open(outputPath, ZipArchiveMode.Create);
                    foreach(var f in files) {
                        zip.CreateEntryFromFile(f, Path.GetRelativePath(path, f), CompressionLevel.Optimal);
                    }
                }
                cache.FileSize = new FileInfo(outputPath).Length;
                cache.Save();
            } else {
                if(fileName != cache.FileName) {
                    Log.Error.OnObject(this, $"Same content available with different names <{fileName}> and <{cache.FileName}>");
                }
            }

            return cache.GetFileUri(cache.FileName).ToString() + "|" + StringUtils.HumanizeSize(cache.FileSize);
        }

        public override Node Generate(Context context) {
            return new Tag("div")
                .Add(new Behaviour("Downloadable", PrimaryName, OptionsTree).Generate(context));
        }
    }
}
