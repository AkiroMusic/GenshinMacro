using System;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: DecompilerTool <assembly-path> [output-directory]");
            return;
        }

        string assemblyPath = args[0];
        string outputDir = args.Length > 1 ? args[1] : Path.GetDirectoryName(assemblyPath) ?? ".";
        
        if (!File.Exists(assemblyPath))
        {
            Console.WriteLine($"File not found: {assemblyPath}");
            return;
        }

        try
        {
            var settings = new DecompilerSettings
            {
                ThrowOnAssemblyResolveErrors = false,
                UsingDeclarations = true,
            };

            var resolver = new UniversalAssemblyResolver(
                Path.GetDirectoryName(assemblyPath) ?? ".",
                false,
                ".dll",
                ".exe"
            );

            var decompiler = new CSharpDecompiler(assemblyPath, resolver, settings);
            var outputPath = Path.Combine(outputDir, "Decompiled");
            
            Directory.CreateDirectory(outputPath);
            
            // Decompile whole module as a single file
            var code = decompiler.DecompileWholeModuleAsString();
            var outputFile = Path.Combine(outputPath, "Decompiled.cs");
            File.WriteAllText(outputFile, code);
            
            Console.WriteLine($"Decompiled to: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}