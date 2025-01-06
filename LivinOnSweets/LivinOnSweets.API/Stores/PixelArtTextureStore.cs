using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // Custom Texture Store whose purpose is to load up Pixel Art assets without having to modify the DEFAULT FILTERING MODE from the global Texture Store
    // I added more arguments to make it more flexible, will probably make a large version of this
    public class PixelArtTextureStore(IRenderer renderer, IResourceStore<TextureUpload> store = null, bool useAtlas = true, bool manualMipmaps = false, float scaleAdjust = 2)
        : TextureStore(renderer, store, useAtlas, TextureFilteringMode.Nearest, manualMipmaps, scaleAdjust) { }
}
