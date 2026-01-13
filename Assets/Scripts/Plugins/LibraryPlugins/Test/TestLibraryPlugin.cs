
using NXD.Plugins.Libraries;

namespace TestLibraryPlugin
{
    public class TestLibraryPlugin : LibraryPlugin
    {
        public override string Name { get; } = "My Test Library";
        public override string Description { get; } = "This is just a test plugin for testing purposes. It loads two games and that's it.";
        public override string IconPath { get; } = "applogo.png";
        
        public override LibraryEntry[] GetEntries()
        {
            return new[]
            {
                new LibraryEntry("Sundermead", "Josephus", "Josephus", "A test library.", "Assets/Plugins/TestLibraryPlugin/TestLibrary.dll")
                {
                    CoverImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1649500/library_600x900.jpg?t=1719410991",
                    BannerImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1649500/capsule_616x353.jpg?t=1719410991",
                    IconPath = "https://shared.fastly.steamstatic.com/community_assets/images/apps/1649500/ff66bee0b28522e24b9dda7f3b0d820caae31127.jpg"
                },
                new LibraryEntry("Moonlight", "Moonlight", "Moonlight", "Stream games innit.", "Assets/Plugins/TestLibraryPlugin/TestLibrary2.dll")
                {
                    CoverImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/library_600x900.jpg",
                    BannerImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/library_hero.jpg",
                    IconPath = "https://shared.fastly.steamstatic.com/community_assets/images/apps/730/8dbc71957312bbd3baea65848b545be9eae2a355.jpg"
                }
            };
        }
    }
}
