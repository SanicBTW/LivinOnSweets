using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    public class MainMenuStore(IRenderer renderer, IResourceStore<byte[]> resources) : AnimatedPixelArtTextureStore(renderer, new TextureLoaderStore(new MainMenuNamespace(resources)), false, true, 1) { }

    internal class MainMenuNamespace(IResourceStore<byte[]> store)
        : PreservingNamespaceResourceStore<byte[]>(store, "MainMenu");
}
