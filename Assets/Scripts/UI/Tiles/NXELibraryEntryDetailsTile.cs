using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;

public class NXELibraryEntryDetailsTile : NXETile
{
    private LibraryEntry libraryEntry;
    private bool isInstalling;
    private GameActionsManager gameActionsManager;

    [SerializeField] private Button installButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Text titleText;
    
    public void SetLibraryEntry(LibraryEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        gameActionsManager = FindFirstObjectByType<GameActionsManager>();
        libraryEntry = entry;
        
        SetIsInstalling(gameActionsManager.IsEntryInstalling(libraryEntry));

        playButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(libraryEntry.Path))
            {
                if(isInstalling == false)
                    installButton.onClick.Invoke();
            }
            else
            {
                var actionManager = FindFirstObjectByType<GameActionsManager>();
                actionManager.LaunchLibraryEntry(libraryEntry);
            }
        });

        installButton.OnClickAsAsyncEnumerable().Subscribe(async au =>
        {
            if (string.IsNullOrEmpty(libraryEntry.Path))
            {
                gameActionsManager.TryInstallLibraryEntry(libraryEntry);
                gameActionsManager.OnInstallationCompleteOrCancelled += ActionManagerOnOnInstallationCompleteOrCancelled;
            }

            SetIsInstalling(true);
        });
        
        titleText.text = libraryEntry.Name;
    }

    void SetIsInstalling(bool value)
    {
        isInstalling = value;
        if (isInstalling)
        {
            installButton.GetComponentInChildren<Text>().text = "Installing...";
            installButton.interactable = false;
            playButton.interactable = false;
        }
        else
        {
            playButton.interactable = true;
            installButton.interactable = true;
            UpdateInstallButtonText();
        }
    }

    void UpdateInstallButtonText()
    {
        installButton.GetComponentInChildren<Text>().text = string.IsNullOrEmpty(libraryEntry.Path) 
            ? "Install" 
            : "Uninstall";
    }

    private void ActionManagerOnOnInstallationCompleteOrCancelled(ObjectId obj)
    {
        if (libraryEntry.Id == obj)
        {
            var entry = FindFirstObjectByType<DatabaseManager>().LibraryEntries.FindOne(x => x.Id == obj);
            SetLibraryEntry(entry);
            
            installButton.interactable = true;
            installButton.Select();
            
            gameActionsManager.OnInstallationCompleteOrCancelled -= ActionManagerOnOnInstallationCompleteOrCancelled;
        }
    }
}
