using System;

namespace LibraryPlugin
{
    public class AdditionalMetadata
    {
        public AdditionalMetadata(string description, string[] screenshotUrls, string[] developers, string[] publishers, string[] genres, DateTime? releaseDate)
        {
            Description = description;
            ScreenshotUrls = screenshotUrls;
            Developers = developers;
            Publishers = publishers;
            Genres = genres;
            ReleaseDate = releaseDate;
        }

        public string Description { get; }
        public string[] ScreenshotUrls { get; }
        public string[] Developers { get; }
        public string[] Publishers { get; }
        public string[] Genres { get; }
        public DateTime? ReleaseDate { get; }
    }
}