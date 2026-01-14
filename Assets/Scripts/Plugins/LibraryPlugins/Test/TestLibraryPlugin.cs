using Cysharp.Threading.Tasks;
using LibraryPlugin;
using System.Collections.Generic;
using System.Threading;

namespace TestLibraryPlugin
{
    public class TestLibraryPlugin : LibraryPlugin.LibraryPlugin
    {
        public override string Name { get; } = "My Test Library";
        public override string Description { get; } = "This is just a test plugin for testing purposes. It loads two games and that's it.";
        public override string IconPath { get; } = "applogo.png";

        public override UniTask<ArtworkCollection> GetArtworkCollection(string entryId, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(new ArtworkCollection());
        }

        public override UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(new List<LibraryEntry>
            {
                new LibraryEntry
                {
                    EntryId = "1",
                    Name = "Sundermead",
                    Developer = "Josephus",
                    Publisher = "Josephus",
                    Path = "Assets/Plugins/TestLibraryPlugin/TestLibrary.dll",
                    CoverImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1649500/library_600x900.jpg?t=1719410991",
                    BannerImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1649500/capsule_616x353.jpg?t=1719410991",
                    IconPath = "https://shared.fastly.steamstatic.com/community_assets/images/apps/1649500/ff66bee0b28522e24b9dda7f3b0d820caae31127.jpg"
                },
                new LibraryEntry
                {
                    EntryId = "2",
                    Name = "Moonlight",
                    Developer = "Moonlight",
                    Publisher = "Moonlight",
                    Path = "Assets/Plugins/TestLibraryPlugin/TestLibrary.dll",
                    CoverImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/library_600x900.jpg",
                    BannerImagePath = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/library_hero.jpg",
                    IconPath = "https://shared.fastly.steamstatic.com/community_assets/images/apps/730/8dbc71957312bbd3baea65848b545be9eae2a355.jpg"
                }
            });
        }
    }
}
