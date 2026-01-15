using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LibrariesButtonsBuilder : MonoBehaviour
{
    [SerializeField] private NXEModal parentModal;
    [SerializeField] private LibraryPluginModal libraryModal;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button existingButton;
    [SerializeField] private Text libraryDescriptionText;

    private void Start()
    {
        var source = FindFirstObjectByType<LibrariesManager>();
        foreach (var sourceLibrary in source.Libraries)
        {
            var button = Instantiate(buttonPrefab, parent).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = sourceLibrary.Name;

            _ = sourceLibrary.GetIconAsync().ContinueWith(icon =>
            {
                button.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            });

            button.onClick.AddListener(() =>
            {
                parentModal.OpenSubModal(libraryModal);
                if (parentModal.SubModal is LibraryPluginModal libModal)
                    libModal.SetLibrary(sourceLibrary);
            });

            var eventTrigger = button.GetComponent<EventTrigger>();
            var selectEvent = eventTrigger.triggers.FirstOrDefault(x => x.eventID == EventTriggerType.Select);
            EventTrigger.TriggerEvent ev;
            
            if (selectEvent != null)
            {
                ev = selectEvent.callback;
            }
            else
            {
                ev = new EventTrigger.TriggerEvent();

                button.GetComponent<EventTrigger>().triggers.Add(new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.Select,
                    callback = ev
                });
            }
            
            ev.AddListener(_ =>
            {
                libraryDescriptionText.text = sourceLibrary.Description;
            });
        }
    }
}