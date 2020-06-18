using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csml {
    public static class CSharpScript {

        public static bool ExecuteCommandLineArguments<T>(string[] args) {
            return ExecuteCommandLineArguments(args, typeof(T));
        }

        public static bool ExecuteCommandLineArguments(string[] args, Type staticType = null) {
            var script = string.Join(" ", args).Replace('~', '"');
            return Eval(script, staticType);
        }

        public static bool Eval(string code, Type staticType = null) {
            string stub = $"using System; public class ScriptContainer {{ public static void Exec() {{{code}}} }}";

            if (staticType != null) {
                stub = $"using static {staticType.FullName};" + stub;
            }

            var compilationData = new MemoryStream();
            var compilationResult = CSharpCompilation.Create("RuntimeScript")
                .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Release))
                .AddReferences(GetMetadataReferencesForThisContext())
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(stub))
                .Emit(compilationData);

            if (compilationResult.Success) {
                var assembly = Assembly.Load(compilationData.ToArray());
                assembly.GetType("ScriptContainer").GetMethod("Exec").Invoke(null, null);
                return true;
            } else {
                foreach (var e in compilationResult.Diagnostics) {
                    Console.WriteLine(e.ToString());
                }
                return false;
            }
        }

        private static HashSet<MetadataReference> GetMetadataReferencesForThisContext() {
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
}
