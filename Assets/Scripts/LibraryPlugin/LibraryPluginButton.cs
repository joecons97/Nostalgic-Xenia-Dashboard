using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LibraryPlugin
{
    public class LibraryPluginButton
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Func<CancellationToken, UniTask> Action { get; set; }
    }
}