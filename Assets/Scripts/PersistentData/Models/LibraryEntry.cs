using LiteDB;
using System;

namespace Assets.Scripts.PersistentData.Models
{
    public class LibraryEntry
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } //Red Dead Redemption 2
        public string Developer { get; set; }//Rockstar Games
        public string Publisher { get; set; } //Rockstar Games
        public string Description { get; set; }
        public string Path { get; set; }
        public DateTimeOffset LastPlayed { get; set; }
        public bool HasSearchedForArtwork { get; set; }

        public string Genre { get; set; }
        public string Rating { get; set; }

        public string SourceId { get; set; }
        public string Source { get; set; }

        public string CoverImagePath { get; set; }
        public string IconPath { get; set; }
        public string BannerImagePath { get; set; }
    }
}
