#if UNITY_EDITOR
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
#endif

using UnityEngine;

[System.Serializable]
public class AssemblyDefinitionData
{
    public string name;
    public string[] references;
}

[CreateAssetMenu(fileName = "PluginDefinition", menuName = "Scriptable Objects/PluginDefinition")]
public class PluginDefinition : ScriptableObject
{
#if UNITY_EDITOR

    [SerializeField] private AssemblyDefinitionAsset assemblyDefinition;
    [SerializeField] private Texture2D icon;

    [ContextMenu("Export")]
    public void Export()
    {
        try
        {
            var outPath = EditorUtility.SaveFilePanel("Export Plugin", "", $"{assemblyDefinition.name}.zip", "zip");

            EditorUtility.DisplayProgressBar("Exporting Plugin", "Gathering Dependencies", 0.3f);

            var pluginAssembly = FindCompiledAssembly(assemblyDefinition.name);
            var pluginInstance = GetLibraryPlugin(pluginAssembly);
            if (pluginInstance == null)
            {
                Debug.LogError("Failed to create plugin instance. Make sure the assembly contains a class that inherits from LibraryPlugin.");
                return;
            }

            var paths = GatherDependencyPaths();
            foreach (var path in paths)
            {
                Debug.Log($"Dependency: {path}");
            }

            paths.Add(pluginAssembly);

            using var fileStream = File.Create(outPath);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, false);

            EditorUtility.DisplayProgressBar("Exporting Plugin", "Archiving Assemblies", 0.6f);

            foreach (var path in paths)
            {
                var name = Path.GetFileName(path);
                EditorUtility.DisplayProgressBar("Exporting Plugin", $"Compressing {name}", 0.9f);

                var entry = archive.CreateEntry(name);
                using var entryStream = entry.Open();
                entryStream.Write(File.ReadAllBytes(path));

            }

            var iconEntry = archive.CreateEntry("icon.png");
            using var iconStream = iconEntry.Open();
            iconStream.Write(icon.EncodeToPNG());

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private LibraryPlugin.LibraryPlugin GetLibraryPlugin(string path)
    {
        using var assemblyDef = AssemblyDefinition.ReadAssembly(path);
        var pluginType = assemblyDef.MainModule.Types.FirstOrDefault(x => x.BaseType?.Name.Contains("LibraryPlugin") == true);

        var instance = Activator.CreateInstance(assemblyDef.FullName, pluginType.FullName).Unwrap() as LibraryPlugin.LibraryPlugin;

        return instance;
    }

    private List<string> GatherDependencyPaths()
    {
        var paths = new List<string>();

        paths.AddRange(GetAssemblyDependencies());

        paths.AddRange(GetAmsDefDependencies());

        var unitask = paths.FirstOrDefault(x => x.Contains("unitask", System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(unitask) == false)
            paths.Remove(unitask);

        var libraryplugin = paths.FirstOrDefault(x => x.Contains("libraryplugin", System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(libraryplugin) == false)
            paths.Remove(libraryplugin);

        return paths;
    }

    private string[] GetAssemblyDependencies()
    {
        var results = new List<string>();

        var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player)
            .FirstOrDefault(x => x.name == assemblyDefinition.name);

        if (playerAssemblies != null)
        {
            string dllPath = playerAssemblies.outputPath;

            // Read assembly metadata without loading it
            using (var assemblyDef = AssemblyDefinition.ReadAssembly(dllPath))
            {
                var references = assemblyDef.MainModule.AssemblyReferences;

                foreach (var reference in references)
                {
                    string refName = reference.Name;

                    // Skip Unity assemblies
                    if (refName.StartsWith("UnityEngine") || refName.StartsWith("UnityEditor"))
                        continue;

                    // Find the DLL in Assets
                    string dllName = refName + ".dll";
                    string[] foundDlls = Directory.GetFiles("Assets/", dllName, SearchOption.AllDirectories);

                    if (foundDlls.Length > 0)
                    {
                        results.Add(foundDlls[0]);
                    }
                }
            }
        }

        return results.ToArray();
    }

    private string[] GetAmsDefDependencies()
    {
        var results = new List<string>();
        // Get the path to the .asmdef file
        string asmDefPath = AssetDatabase.GetAssetPath(assemblyDefinition);

        // Read and parse the JSON
        string json = File.ReadAllText(asmDefPath);
        AssemblyDefinitionData asmDefData = JsonUtility.FromJson<AssemblyDefinitionData>(json);

        if (asmDefData.references != null && asmDefData.references.Length > 0)
        {
            foreach (string reference in asmDefData.references)
            {
                // Reference format is "GUID:xxxxx"
                string guid = reference.Replace("GUID:", "");

                // Convert GUID to asset path
                string refPath = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(refPath))
                {
                    // Load and read the referenced .asmdef
                    string refJson = File.ReadAllText(refPath);
                    AssemblyDefinitionData refData = JsonUtility.FromJson<AssemblyDefinitionData>(refJson);

                    string dllPath = FindCompiledAssembly(refData.name);
                    if (!string.IsNullOrEmpty(dllPath))
                    {
                        results.Add(dllPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find compiled assembly for: {refData.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find asset for GUID: {guid}");
                }
            }
        }

        return results.ToArray();
    }

    private static string FindCompiledAssembly(string assemblyName)
    {
        Assembly[] assemblies = CompilationPipeline.GetAssemblies();
        Assembly assembly = assemblies.FirstOrDefault(a => a.name == assemblyName);
        return assembly?.outputPath;
    }
#endif
}
