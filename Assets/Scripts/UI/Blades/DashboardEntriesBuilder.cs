using Assets.Scripts.PersistentData.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DashboardEntriesBuilder : MonoBehaviour
{
    [Header("Blade"), SerializeField] private NXEBlade bladePrefab;
    [SerializeField] private NXEBlade libraryEntriesBladePrefab;

    [Header("Tiles"), SerializeField] private NXETile defaultTilePrefab;
    [SerializeField] private NXELibraryEntryTile libraryEntryTilePrefab;

    private DatabaseManager databaseManager;
    private List<NXEBlade> spawnedBlades = new();

    void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        foreach (var blade in spawnedBlades)
        {
            if (blade == null)
                continue;
            
            Destroy(blade.gameObject);
        }

        spawnedBlades.Clear();

        databaseManager ??= FindFirstObjectByType<DatabaseManager>();

        foreach (var entry in databaseManager.DashboardEntries.FindAll())
        {
            switch (entry.Type)
            {
                case DashboardEntryType:
                    BuildDashboardEntryType(entry);
                    break;
            }
        }
    }

    private NXEBlade BuildDashboardEntryType(DashboardEntry entry)
    {
        var blade = Instantiate(libraryEntriesBladePrefab, transform);
        blade.name = $"{entry.Name}";
        spawnedBlades.Add(blade);
        blade.SetTitle(entry.Name);

        //LiteDB doesn't support ThenBy so we have to do it locally
        var entries = databaseManager.LibraryEntries.Query()
            .Where(x => x.Source == entry.Data)
            .ToArray()
            .OrderByDescending(x => x.LastPlayed)
            .ThenBy(x => x.Name)
            .ToArray();

        blade.SetTiles(entries.Select(x => libraryEntryTilePrefab).ToArray<NXETile>());

        int index = 0;
        foreach (var tile in blade.Tiles)
        {
            if (tile is NXELibraryEntryTile libEntry)
            {
                libEntry.SetLibraryEntry(entries[index]);
                index++;
            }
        }

        return blade;
    }

    public void RebuildDashboardEntry(string dashboardName)
    {
        var entry = databaseManager.DashboardEntries.FindOne(x => x.Name == dashboardName);
        if (entry == null)
            return;

        int index = 0;
        var existingBlade = transform.Find(dashboardName);
        if (existingBlade != null)
            index = existingBlade.GetSiblingIndex();

        spawnedBlades.Remove(existingBlade.GetComponent<NXEBlade>());
        Destroy(existingBlade.gameObject);

        var newBlade = BuildDashboardEntryType(entry);
        spawnedBlades.Add(newBlade);
        newBlade.transform.SetSiblingIndex(index);
    }
}
