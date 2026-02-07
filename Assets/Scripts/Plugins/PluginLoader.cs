using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PluginLoader : MonoBehaviour
{
    public static string PluginsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NXD",
        "Plugins");

    private static string TempPath => Path.Combine(PluginsPath, "Temp");
    private const string ICON_NAME = "icon.png";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Directory.Exists(PluginsPath) == false)
            Directory.CreateDirectory(PluginsPath);

        CleanUpTempFolder();

        if (Directory.Exists(TempPath) == false)
            Directory.CreateDirectory(TempPath);

        Debug.Log($"Plugins Path: {PluginsPath}");
    }

    private void CleanUpTempFolder()
    {
        if (Directory.Exists(TempPath))
        {
            try
            {
                Directory.Delete(TempPath, true);
                Debug.Log("Cleaned up temporary plugin folder.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clean up temporary plugin folder");
                Debug.LogException(e);
            }
        }
    }

    public async UniTask<Library[]> LoadLibraryPluginsAsync()
    {
        var plugins = new List<Library>();

        ExtractArchives();

        var dlls = Directory.GetFiles(TempPath, "*.dll", SearchOption.AllDirectories);

        // Track plugin directories for dependency resolution
        var pluginDirectories = new HashSet<string>();

        AppDomain.CurrentDomain.AssemblyResolve += resolveAssembly;

        foreach (var dll in dlls)
        {
            try
            {
                // Track this plugin's directory
                var pluginDir = Path.GetDirectoryName(dll);
                if (!string.IsNullOrEmpty(pluginDir))
                {
                    pluginDirectories.Add(pluginDir);
                }

                var asm = Assembly.LoadFile(dll);
                var entry = asm.GetExportedTypes().FirstOrDefault(x => typeof(LibraryPlugin.LibraryPlugin).IsAssignableFrom(x));
                if (entry == null) continue;

                var plugin = (LibraryPlugin.LibraryPlugin)Activator.CreateInstance(entry);
                plugins.Add(new Library(plugin.Name, plugin.Description, Path.Combine(pluginDir ?? string.Empty, ICON_NAME), dll, plugin));
                await plugin.OnPluginLoaded();
                Debug.Log($"Loaded Plugin: {asm.GetName()}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load plugin: {dll}");
                Debug.LogException(e);
            }
        }


        Assembly resolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            // Search in all plugin directories
            foreach (var dir in pluginDirectories)
            {
                var assemblyPath = Path.Combine(dir, assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    Debug.Log($"Resolved dependency: {assemblyName.Name} from {assemblyPath}");
                    return Assembly.LoadFile(assemblyPath);
                }
            }

            return null;
        }

        AppDomain.CurrentDomain.AssemblyResolve -= resolveAssembly;

        return plugins.ToArray();
    }

    private void ExtractArchives()
    {
        var archives = Directory.GetFiles(PluginsPath, "*.zip", SearchOption.AllDirectories);

        foreach (var item in archives)
        {
            var archiveName = Path.GetFileNameWithoutExtension(item);
            var directory = Path.Combine(TempPath, archiveName);

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            ZipFile.ExtractToDirectory(item, directory, overwriteFiles: true);

            Debug.Log($"Extracted plugin archive: {item}");
        }
    }

    private void OnDestroy()
    {
        CleanUpTempFolder();
    }
}