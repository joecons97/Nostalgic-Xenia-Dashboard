using System.Collections.Generic;
using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Extensions;
using LiteDB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LibraryPluginModal : NXEModal
{
    private Library library;
    private LibrariesManager librariesManager;
    private DatabaseManager databaseManager;

    [SerializeField] private Text infoText;
    [SerializeField] private Button importButton;
    [SerializeField] private Text importText;
    [SerializeField] private Button toggleDashboardButton;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Text customDescriptionField;

    private List<Button> spawnedButtons = new List<Button>();

    private string originalImportText;

    private void Start()
    {
        importButton.OnClickAsAsyncEnumerable().Subscribe(async (asyncUnit) => await DoReImporter());
        toggleDashboardButton.onClick.AddListener(ToggleDashboard);

        originalImportText = importText.text;

        librariesManager.OnLibraryImportEnd += LibrariesManager_OnLibraryImportEnd;
        librariesManager.ImportProgress.OnProgressed += ImportProgress_OnProgressed; 
    }

    void OnEnable()
    {
        UniTask.WaitUntil(() => library != null, cancellationToken: this.destroyCancellationToken)
            .ContinueWith(() =>
        {
            foreach (var button in library.Plugin.GetButtons())
            {
                var newButton = Instantiate(buttonPrefab, buttonsParent);
                newButton.GetComponentInChildren<Text>().text = button.Name;

                if (button.Action != null)
                    newButton.OnClickAsAsyncEnumerable().Subscribe(async (asyncUnit) => await button.Action(this.GetCancellationTokenOnDestroy()));

                var eventTrigger = newButton.GetComponent<EventTrigger>();
                
                eventTrigger.triggers.Add(new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.Select,
                    callback = new EventTrigger.TriggerEvent().Listen(ev => customDescriptionField.text = button.Description)
                });
                
                eventTrigger.triggers.Add(new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.Deselect,
                    callback = new EventTrigger.TriggerEvent().Listen(ev => customDescriptionField.text = "")
                });
                
                spawnedButtons.Add(newButton);
            }
        }).Forget();
    }

    void OnDisable()
    {
        foreach(var btn in spawnedButtons)
        {
            if(btn)
                Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();
    }

    private void ImportProgress_OnProgressed(Progress obj)
    {
        importText.gameObject.SetActive(true);
        importText.text = $"{Mathf.Floor(obj.PercentageComplete * 100)}% - {obj.Status}";
    }

    private void OnDestroy()
    {
        librariesManager.OnLibraryImportEnd -= LibrariesManager_OnLibraryImportEnd;
        librariesManager.ImportProgress.OnProgressed -= ImportProgress_OnProgressed;
    }

    private void LibrariesManager_OnLibraryImportEnd(Library lib)
    {
        if (lib == library)
        {
            importButton.interactable = true;
            importButton.Select();
            ResetDescriptions();
            canBeClosed = true;

            FindFirstObjectByType<DashboardEntriesBuilder>().Rebuild();
        }
    }

    public void SetLibrary(Library lib)
    {
        if (lib == null)
        {
            Debug.LogWarning("LibraryPluginModal: SetLibrary called with null library.");
            return;
        }

        librariesManager = FindFirstObjectByType<LibrariesManager>();
        databaseManager = FindFirstObjectByType<DatabaseManager>();

        library = lib;

        titleText.text = library != null ? library.Name : "Library Plugin";

        ResetDescriptions();

        HandleImportButton();

        if (databaseManager.DashboardEntries.Exists(x => x.Type == DashboardEntryType.LibraryEntrySource && x.Name == library.Name))
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Remove from Dashboard";
        else
            toggleDashboardButton.GetComponentInChildren<Text>().text = "Add to Dashboard";
    }

    void ResetDescriptions()
    {
        infoText.text = library != null
            ? $"Name: {library.Name}\nDescription: {library.Description}"
            : "No library information available.";

        importText.text = originalImportText;
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

    public UniTask DoReImporter()
    {
        canBeClosed = false;
        _ = librariesManager.ImportLibraryAsync(library);
        HandleImportButton();
        
        return UniTask.CompletedTask;
    }

    void HandleImportButton()
    {
        importButton.interactable = librariesManager.ActivelyImportingLibrary != library;
    }
}
