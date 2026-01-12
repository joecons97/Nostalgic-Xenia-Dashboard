namespace NXD.Plugins.Libraries
{
    public abstract class LibraryPlugin
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string IconPath { get; }
        public abstract LibraryEntry[] GetEntries();
    }
}
