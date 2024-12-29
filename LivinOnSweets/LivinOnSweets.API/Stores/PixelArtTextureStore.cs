using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // Custom Texture Store whose purpose is to load up Pixel Art assets without having to modify the DEFAULT FILTERING MODE from the global Texture Store
    public class PixelArtTextureStore(IRenderer renderer, IResourceStore<TextureUpload> store = null)
        : TextureStore(renderer, store, true, TextureFilteringMode.Nearest, false)
    {
    }
}
