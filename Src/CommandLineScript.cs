using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class CommandLineScript {
    public static Task<bool> WarmUpAsync() {
        return Task<bool>.Run(() => WarmUp());

    }
    public static bool WarmUp() {
        return ExecuteScript("", null);
    }

    public static bool ExecuteCommandLineArguments<T>(string[] args) {
        return ExecuteCommandLineArguments(args, typeof(T));
    }
    public static bool ExecuteCommandLineArguments(string[] args, Type staticType = null) {
        var script = string.Join(" ", args).Replace('~', '"');
        return ExecuteScript(script, staticType);
    }


    public static bool ExecuteScript(string script, Type staticType = null) {
        string wrapper = $"using System; public class ScriptContainer {{ public static void Exec() {{{script}}} }}";
        if (staticType != null) {
            wrapper = $"using static {staticType.FullName};" + wrapper;
        }

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(wrapper);

        string fileName = "script.dll";
        var compilation = CSharpCompilation.Create(fileName)
            .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Release))
            .AddReferences(GetMetadataReferencesForThisContext())
            .AddSyntaxTrees(syntaxTree);
        MemoryStream library = new MemoryStream();
        EmitResult compilationResult = compilation.Emit(library);
        if (compilationResult.Success) {
            Assembly assembly = Assembly.Load(library.ToArray());// AssemblyLoadContext.Default.LoadFromStream(library);
            assembly.GetType("ScriptContainer").GetMethod("Exec").Invoke(null, null);
            return true;
        } else {
            foreach (var e in compilationResult.Diagnostics) {
                Console.WriteLine(e.ToString());
            }
            return false;
        }
    }

    static HashSet<MetadataReference> GetMetadataReferencesForThisContext() {
        var result = new HashSet<MetadataReference>();
        var systemRuntime = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(x => x.Name == "System.Runtime");

        result.Add(MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location));
        result.Add(MetadataReference.CreateFromFile(Assembly.Load(systemRuntime).Location));

        var types = new Type[] { typeof(object), typeof(Console) };
        foreach (var t in types) {
            result.Add(MetadataReference.CreateFromFile(t.GetTypeInfo().Assembly.Location));
        }
        return result;
    }
}