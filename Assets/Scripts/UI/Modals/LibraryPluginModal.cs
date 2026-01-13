using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LiteDB;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LibraryPluginModal : NXEModal
{
    private Library library;
    private LibrariesManager librariesManager;
    private DatabaseManager databaseManager;

    [SerializeField] private Text infoText;
    [SerializeField] private Button importButton;
    [SerializeField] private Button toggleDashboardButton;

    private void Start()
    {
        importButton.OnClickAsAsyncEnumerable().Subscribe(async (asyncUnit) => await DoReImporter());
        toggleDashboardButton.onClick.AddListener(ToggleDashboard);

        librariesManager.OnLibraryImportEnd += LibrariesManager_OnLibraryImportEnd;
    }

    private void OnDestroy()
    {
        librariesManager.OnLibraryImportEnd -= LibrariesManager_OnLibraryImportEnd;
    }

    private void LibrariesManager_OnLibraryImportEnd(Library lib)
    {
        if (lib == library)
        {
            importButton.interactable = true;
            importButton.Select();
        }
    }

    public void SetLibrary(Library lib)
    {
        if(lib == null)
        {
            Debug.LogWarning("LibraryPluginModal: SetLibrary called with null library.");
            return;
        }

        librariesManager = FindFirstObjectByType<LibrariesManager>();
        databaseManager = FindFirstObjectByType<DatabaseManager>();

        library = lib;

        titleText.text = library != null ? library.Name : "Library Plugin";

        infoText.text = library != null
            ? $"Name: {library.Name}\nGames Found: {library.Entries.Length}\nDescription: {library.Description}"
            : "No library information available.";

        HandleImportButton();

        if (databaseManager.DashboardEntries.Exists(x => x.Type == DashboardEntryType.LibraryEntrySource && x.Name == library.Name))
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Remove from Dashboard";
        else
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Add to Dashboard";
    }

    public void ToggleDashboard()
    {
        var entry = databaseManager.DashboardEntries.FindOne(x => x.Type == DashboardEntryType.LibraryEntrySource && x.Name == library.Name);
        if (entry != null)
        {
            databaseManager.DashboardEntries.Delete(entry.Id);
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Add to Dashboard";

        }
        else
        {
            entry = new DashboardEntry()
            {
                Id = ObjectId.NewObjectId(),
                Name = library.Name,
                Data = library.Name,
                Type = DashboardEntryType.LibraryEntrySource,
            };
            databaseManager.DashboardEntries.Insert(entry);
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Remove from Dashboard";
        }

        FindFirstObjectByType<DashboardEntriesBuilder>().Rebuild();
    }

    public async UniTask DoReImporter()
    {
        _ = librariesManager.ImportLibraryAsync(library);
        HandleImportButton();
    }

    void HandleImportButton()
    {
        importButton.interactable = librariesManager.ActivelyImportingLibrary != library;
    }
}
