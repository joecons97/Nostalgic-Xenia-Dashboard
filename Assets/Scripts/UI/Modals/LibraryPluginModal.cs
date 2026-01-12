using UnityEngine;
using UnityEngine.UI;

public class LibraryPluginModal : NXEModal
{
    private Library library;

    [SerializeField] private Text infoText;

    public void SetLibrary(Library lib)
    {
        if(lib == null)
        {
            Debug.LogWarning("LibraryPluginModal: SetLibrary called with null library.");
            return;
        }

        library = lib;

        titleText.text = library != null ? library.Name : "Library Plugin";

        infoText.text = library != null
            ? $"Name: {library.Name}\nGames Found: {library.Entries.Length}\nDescription: {library.Description}"
            : "No library information available.";
    }

    public void DoReImporter()
    {

    }
}
