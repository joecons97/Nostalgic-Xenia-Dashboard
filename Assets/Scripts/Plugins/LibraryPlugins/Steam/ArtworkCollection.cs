namespace SteamLibraryPlugin
{
    public class ArtworkCollection
    {
        public string Cover { get; set; }
        public string Banner { get; set; }
        public string Icon { get; set; }

        public bool IsComplete => string.IsNullOrEmpty(Cover) && string.IsNullOrEmpty(Banner) && string.IsNullOrEmpty(Icon);
    }
}
