using UnityEngine;

public class LibrariesManager : MonoBehaviour
{
    [SerializeField] private PluginLoader loader;

    public Library[] Libraries { get; private set; }

    private void Start()
    {
        Libraries = loader.LoadLibraryPlugins();
    }
}
