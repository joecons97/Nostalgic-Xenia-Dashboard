using Assets.Scripts.PersistentData.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DashboardEntriesBuilder : MonoBehaviour
{
    [SerializeField] private NXEBlade BladePrefab;
    [Header("Tiles"), SerializeField] private NXETile defaultTilePrefab;
    [SerializeField] private NXELibraryEntryTile libraryEntryTile;

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
            var blade = Instantiate(BladePrefab, transform);
            spawnedBlades.Add(blade);
            blade.SetTitle(entry.Name);

            switch (entry.Type)
            {
                case DashboardEntryType:
                    var entries = databaseManager.LibraryEntries.Find(x => x.Source == entry.Data)
                        .ToArray();

                    blade.SetTiles(entries.Select(x => libraryEntryTile).ToArray());

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
