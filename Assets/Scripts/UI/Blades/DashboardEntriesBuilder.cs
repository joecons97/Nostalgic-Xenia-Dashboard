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
            Destroy(blade.gameObject);
        }

        spawnedBlades.Clear();

        databaseManager ??= FindFirstObjectByType<DatabaseManager>();

        foreach (var entry in databaseManager.DashboardEntries.FindAll())
        {
            switch (entry.Type)
            {
                case DashboardEntryType:

                    var blade = Instantiate(libraryEntriesBladePrefab, transform);
                    spawnedBlades.Add(blade);
                    blade.SetTitle(entry.Name);

                    var entries = databaseManager.LibraryEntries.Query()
                        .Where(x => x.Source == entry.Data)
                        .OrderByDescending(x => x.LastPlayed)
                        .ToArray();

                    blade.SetTiles(entries.Select(x => libraryEntryTilePrefab).ToArray());

                    int index = 0;
                    foreach (var tile in blade.Tiles)
                    {
                        if (tile is NXELibraryEntryTile libEntry)
                        {
                            libEntry.SetLibraryEntry(entries[index]);
                            index++;
                        }
                    }
                    break;
            }
        }
    }
}
