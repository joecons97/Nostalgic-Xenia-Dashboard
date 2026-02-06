using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LibraryPlugin
{
    public abstract class LibraryPlugin
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public Func<string, LibraryPlugin, UniTask> OnEntryProcessEnded;
        public Func<string, string, LibraryPlugin, UniTask> OnEntryInstallationComplete;
        public Func<string, LibraryPlugin, UniTask> OnEntryInstallationCancelled;

        public Func<string, LibraryPlugin, UniTask> OnEntryUninstallationComplete;
        public Func<string, LibraryPlugin, UniTask> OnEntryUninstallationCancelled;

        public abstract UniTask<List<LibraryEntry>> GetEntriesAsync(CancellationToken cancellationToken);
        public abstract UniTask<ArtworkCollection> GetArtworkCollection(string entryId, CancellationToken cancellationToken);

        public virtual UniTask OnPluginLoaded()
        {
            return UniTask.CompletedTask;
        }
        
        public virtual UniTask<AdditionalMetadata> GetAdditionalMetadata(string entryId, CancellationToken cancellationToken)
        {
            return UniTask.FromResult<AdditionalMetadata>(null);
        }

        public virtual UniTask<GameActionResult> TryStartEntryAsync(LibraryEntry entry, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(GameActionResult.Indeterminate);
        }

        public virtual UniTask<GameActionResult> TryInstallEntryAsync(LibraryEntry entry, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(GameActionResult.Indeterminate);
        }

        public virtual UniTask<GameActionResult> TryUninstallEntryAsync(LibraryEntry entry, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(GameActionResult.Indeterminate);
        }

        public virtual List<LibraryPluginButton> GetButtons()
        {
            return new List<LibraryPluginButton>();
        }

        public virtual UniTask OpenLibraryApplication(LibraryLocation location)
        {
            return UniTask.CompletedTask;
        }
    }
}