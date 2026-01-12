namespace NXD.Plugins.Libraries
{
    public class LibraryEntry
    {
        public string Name { get; } //Red Dead Redemption 2
        public string Developer { get; } //Rockstar Games
        public string Publisher { get; } //Rockstar Games
        public string Description { get; }
        public string Path { get; }

        public string Genre { get; set; }
        public string Rating { get; set; }

        public string Source { get; set; }

        public string CoverImagePath { get; set; }
        public string IconPath { get; set; }
        public string BannerImagePath { get; set; }
        
        public LibraryEntry(string name, string developer, string publisher, string description, string path)
        {
            Name = name;
            Developer = developer;
            Publisher = publisher;
            Description = description;
            Path = path;
        }
    }
}