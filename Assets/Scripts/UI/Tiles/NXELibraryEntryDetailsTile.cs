using System;
using Assets.Scripts.PersistentData.Models;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;

public class NXELibraryEntryDetailsTile : NXETile
{
    private LibraryEntry libraryEntry;
    private bool isOperant;
    private GameActionsManager gameActionsManager;

    [SerializeField] private Button installButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Text titleText;

    private void Awake()
    {
        gameActionsManager = FindFirstObjectByType<GameActionsManager>();
    }

    private void Start()
    {
        gameActionsManager.OnInstallationCompleteOrCancelled += GameActionsManagerOnOperationCompleteOrCancelled;
        gameActionsManager.OnUninstallationCompleteOrCancelled += GameActionsManagerOnOperationCompleteOrCancelled;
        gameActionsManager.OnInstallationBegin += GameActionsManagerOnOperandBegin;
        gameActionsManager.OnUninstallationBegin += GameActionsManagerOnOperandBegin;
    }

    private void OnDestroy()
    {
        gameActionsManager.OnInstallationCompleteOrCancelled -= GameActionsManagerOnOperationCompleteOrCancelled;
        gameActionsManager.OnUninstallationCompleteOrCancelled -= GameActionsManagerOnOperationCompleteOrCancelled;
        gameActionsManager.OnInstallationBegin -= GameActionsManagerOnOperandBegin;
        gameActionsManager.OnUninstallationBegin -= GameActionsManagerOnOperandBegin;
    }

    public void SetLibraryEntry(LibraryEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("SetLibraryEntry null?");
            return;
        }

        libraryEntry = entry;
        
        SetIsOperant(gameActionsManager.IsEntryOperant(libraryEntry));

        playButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(libraryEntry.Path))
            {
                if(isOperant == false)
                    installButton.onClick.Invoke();
            }
            else
            {
                var actionManager = FindFirstObjectByType<GameActionsManager>();
                actionManager.LaunchLibraryEntry(libraryEntry);
            }
        });

        installButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(libraryEntry.Path))
            {
                gameActionsManager.TryInstallLibraryEntry(libraryEntry);
            }
            else
            {
                gameActionsManager.TryUninstallLibraryEntry(libraryEntry);
            }
        });
        
        titleText.text = libraryEntry.Name;
    }

    void SetIsOperant(bool value)
    {
        isOperant = value;
        if (isOperant)
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

    private void GameActionsManagerOnOperationCompleteOrCancelled(ObjectId obj)
    {
        if (libraryEntry.Id == obj)
        {
            var entry = FindFirstObjectByType<DatabaseManager>().LibraryEntries.FindOne(x => x.Id == obj);
            SetLibraryEntry(entry);
            
            installButton.interactable = true;
            installButton.Select();
        }
    }
    
    private void GameActionsManagerOnOperandBegin(ObjectId obj)
    {
        if (libraryEntry != null && libraryEntry.Id == obj)
            SetIsOperant(true);
    }
}
