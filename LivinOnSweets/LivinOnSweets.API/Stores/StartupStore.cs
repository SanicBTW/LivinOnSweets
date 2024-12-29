using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // Wrapper for PreservingNamespaceResourceStore targetting Startup resource namespace
    // Used to pass it thru the dependency container easily
    public class StartupStore(IResourceStore<byte[]> store) : PreservingNamespaceResourceStore<byte[]>(store, "Startup")
    {
    }
}
