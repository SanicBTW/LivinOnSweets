using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // Wrapper for PreservingNamespaceResourceStore targetting StoryMode resource namespace
    // Used to pass it thru the dependency container easily
    public class StoryModeStore(IResourceStore<byte[]> store) : PreservingNamespaceResourceStore<byte[]>(store, "StoryMode")
    {
    }
}
