
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
                new LibraryEntry("Sundermead", "Josephus", "Josephus", "A test library.", "Assets/Plugins/TestLibraryPlugin/TestLibrary.dll"),
                new LibraryEntry("Moonlight", "Moonlight", "Moonlight", "Stream games innit.", "Assets/Plugins/TestLibraryPlugin/TestLibrary2.dll")
            };
        }
    }
}
