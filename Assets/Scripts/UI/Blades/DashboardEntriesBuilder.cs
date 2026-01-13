using System.Collections.Generic;
using UnityEngine;

public class DashboardEntriesBuilder : MonoBehaviour
{
    [SerializeField] private NXEBlade BladePrefab;

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
        }
    }
}
