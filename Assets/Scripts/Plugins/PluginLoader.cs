using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PluginLoader : MonoBehaviour
{
    public static string PluginsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NXD",
        "Plugins");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Directory.Exists(PluginsPath) == false)
            Directory.CreateDirectory(PluginsPath);

        Debug.Log($"Plugins Path: {PluginsPath}");
    }

    public async UniTask<Library[]> LoadLibraryPluginsAsync()
    {
        var plugins = new List<Library>();
        var dlls = Directory.GetFiles(PluginsPath, "*.dll", SearchOption.AllDirectories);

        // Track plugin directories for dependency resolution
        var pluginDirectories = new HashSet<string>();

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
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
        };

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
                plugins.Add(new Library(plugin.Name, plugin.Description, Path.Combine(pluginDir ?? string.Empty, plugin.IconPath), dll, plugin));
                await plugin.OnPluginLoaded();
                Debug.Log($"Loaded Plugin: {asm.GetName()}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load plugin: {dll}");
                Debug.LogException(e);
            }
        }

        return plugins.ToArray();
    }
}