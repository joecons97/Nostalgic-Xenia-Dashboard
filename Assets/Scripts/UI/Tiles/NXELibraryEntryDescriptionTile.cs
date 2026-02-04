using LibraryPlugin;
using UnityEngine;
using UnityEngine.UI;

public class NXELibraryEntryDescriptionTile : MonoBehaviour
{
    [SerializeField] private Text detailsText;
    [SerializeField] private NXEThrobber throbber;
    
    private AdditionalMetadata metadata;

    private void Awake()
    {
        throbber.gameObject.SetActive(true);
        detailsText.gameObject.SetActive(false);
    }

    public void SetLibraryMetadataEntry(AdditionalMetadata data)
    {
        metadata = data;

        detailsText.text = data?.Description ?? "No description available.";
        
        throbber.gameObject.SetActive(false);
        detailsText.gameObject.SetActive(true);
    }
}
