using System;

namespace Loadables
{
    public interface ILoadable
    {
        event Action<ILoadable> OnLoadComplete;
    }
}