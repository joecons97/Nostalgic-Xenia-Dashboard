using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NXD.Plugins.Libraries;
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
        if(Directory.Exists(PluginsPath) == false)
            Directory.CreateDirectory(PluginsPath);
        
        Debug.Log($"Plugins Path: {PluginsPath}");
    }
    
    public Library[] LoadLibraryPlugins()
    {
        var plugins = new List<Library>();
        var dlls = Directory.GetFiles(PluginsPath, "*.dll", SearchOption.AllDirectories);
        
        foreach (var dll in dlls)
        {
            Debug.Log($"Loading Plugin: {dll}");
            var asm = Assembly.LoadFile(dll);
            var entry = asm.GetExportedTypes().FirstOrDefault(x => typeof(LibraryPlugin).IsAssignableFrom(x));
            if (entry != null)
            {
                var plugin = (LibraryPlugin)Activator.CreateInstance(entry);
                plugins.Add(new Library(plugin.Name, plugin.Description, Path.Combine(Path.GetDirectoryName(dll) ?? string.Empty, plugin.IconPath), plugin.GetEntries()));
            }
        }

        return plugins.ToArray();
    }
}
