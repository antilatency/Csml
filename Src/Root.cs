using Csml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

[GetOnce]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
partial class Root {

    private IEnumerable<IPage> GetPages() { 
        return GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.PropertyType.ImplementsInterface(typeof(IPage))).Select(x => x.GetValue(this) as IPage);
    }

    public void Verify() {
        GetPages().ToList();
    }

    public void Generate(Context context, bool clean = true) {
        if (clean) {
            if (Directory.Exists(context.OutputRootDirectory))
                Directory.Delete(context.OutputRootDirectory, true);
        }

        foreach (var i in context.AssetsToCopy) {
            var dest = Path.Combine(context.OutputRootDirectory, Context.GetContentRelativePath(i, context.SourceRootDirectory));
            Utils.CreateDirectories(Path.GetDirectoryName(dest));
            if (!File.Exists(dest) | context.ForceRebuildAssets)
                File.Copy(i, dest, true);
        }

        var pages = GetPages();
        
        var languages = Language.All;

        var matrix = new Dictionary<string, Dictionary<Language, IPage>>();

        foreach (var p in pages) {
            if (!matrix.ContainsKey(p.NameWithoutLanguage)) {
                matrix.Add(p.NameWithoutLanguage, new Dictionary<Language, IPage>());
            }
            var n = matrix[p.NameWithoutLanguage];
            if (p.Language == null) {
                foreach (var l in languages) {
                    n.Add(l, p);
                }
            } else {
                n.Add(p.Language, p);
            }
        }

        foreach (var n in matrix) {
            if (n.Value.Count != languages.Count) {
                var replacementPage = n.Value[languages.First(x => n.Value.ContainsKey(x))];
                foreach (var l in languages) {
                    if (!n.Value.ContainsKey(l))
                        n.Value.Add(l, replacementPage);
                }
            }
        }

        foreach (var n in matrix) {
            foreach (var l in n.Value) {
                context.Language = l.Key;
                l.Value.Create(context);
            }
        }

    }
}