using Assets.Scripts.PersistentData.Models;
using LiteDB;
using System;
using System.IO;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static string DatabasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NXD",
        "Data");

    private LiteDatabase dbContext;

    public LiteDatabase DbContext => dbContext;
    
    public ILiteCollection<LibraryEntry> LibraryEntries => dbContext
        .GetCollection<LibraryEntry>("library_entries");

    public ILiteCollection<DashboardEntry> DashboardEntries => dbContext
        .GetCollection<DashboardEntry>("dashboard_entries");
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(Directory.Exists(DatabasePath) == false)
        {
            Directory.CreateDirectory(DatabasePath);
        }

        dbContext = new LiteDatabase(Path.Combine(DatabasePath, "nxd_data.db"));

    }

    private void OnDestroy()
    {
        dbContext.Dispose();
    }
}
