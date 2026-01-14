using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace NXD.Plugins.Libraries
{
    public abstract class LibraryPlugin
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string IconPath { get; }
        public abstract UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken);

        //public abstract UniTask<List<AdditionalMetadata>> GetAdditionalMetadata(CancellationToken cancellationToken);
        //public abstract UniTask<List<ArtworkCollection>> GetArtworkCollection(CancellationToken cancellationToken);
    }
}
