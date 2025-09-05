using osu.Framework.Audio;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace LivinOnSweets.API.IO
{
    // https://github.com/ppy/osu/blob/master/osu.Game/IO/IStorageResourceProvider.cs#L12
    public interface IStorageResourceProvider
    {
        /// <summary>
        /// The game renderer.
        /// </summary>
        IRenderer Renderer { get; }

        /// <summary>
        /// Retrieve the game-wide audio manager.
        /// </summary>
        AudioManager AudioManager { get; }

        /// <summary>
        /// Access the Resource Pack files.
        /// </summary>
        IResourceStore<byte[]> Resources { get; }

        /// <summary>
        /// Access to the user folder scoped inside the "resourcepacks" directory.
        /// </summary>
        Storage Storage { get; }

        /// <summary>
        /// Create a texture loader store based on an underlying data store.
        /// </summary>
        /// <param name="underlyingStore">The underlying provider of texture data (in arbitrary image formats).</param>
        /// <returns>A texture loader store.</returns>
        IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore);
    }
}
