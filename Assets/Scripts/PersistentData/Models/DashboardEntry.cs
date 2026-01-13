using LiteDB;

namespace Assets.Scripts.PersistentData.Models
{
    public class DashboardEntry
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public DashboardEntryType Type { get; set; }
        public string Data { get; set; }
    }

    public enum DashboardEntryType
    {
        LibraryEntrySource = 0
    }
}
