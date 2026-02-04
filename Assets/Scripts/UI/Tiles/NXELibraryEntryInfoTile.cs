using System;
using LibraryPlugin;
using UnityEngine;
using UnityEngine.UI;

public class NXELibraryEntryInfoTile : MonoBehaviour
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

        detailsText.text = detailsText.text
            .Replace("{0}", metadata == null || metadata.Developers.Length == 0 ? "-" : $"<i>{string.Join(", ", metadata.Developers)}</i>")
            .Replace("{1}", metadata == null || metadata.Publishers.Length == 0 ? "-" : $"<i>{string.Join(", ", metadata.Publishers)}</i>")
            .Replace("{2}", metadata?.ReleaseDate == null ? "-" : $"<i>{metadata.ReleaseDate.Value.ToLongDateString()}</i>")
            .Replace("{3}", metadata == null || metadata.Genres.Length == 0 ? "-" : $"<i>{string.Join(", ", metadata.Genres)}</i>");
        
        throbber.gameObject.SetActive(false);
        detailsText.gameObject.SetActive(true);
    }
}
