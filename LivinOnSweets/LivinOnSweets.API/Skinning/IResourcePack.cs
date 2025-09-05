using LivinOnSweets.API.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;

namespace LivinOnSweets.API.Skinning
{
    /// <summary>
    /// Provides access to various elements contained by a <see cref="ResourcePack"/>
    /// </summary>
    public interface IResourcePack
    {
        /// <summary>
        /// Returns this pack <see cref="ResourcePackInfo"/>.
        /// </summary>
        ResourcePackInfo PackInfo { get; }

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <param name="wrapModeS">The texture wrap mode in horizontal direction.</param>
        /// <param name="wrapModeT">The texture wrap mode in vertical direction.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) =>
            GetTexture(componentName, wrapModeS, wrapModeT, true);

        /// <summary>
        /// Retrieves a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The name of the texture.</param>
        /// <param name="wrapModeS">The texture wrap mode in horizontal direction.</param>
        /// <param name="wrapModeT">The texture wrap mode in vertical direction.</param>
        /// <param name="useAtlas">True if it should try to add the texture to the internal Atlas.</param>
        /// <param name="manualMipmaps">True if it should retrieve the texture without any mipmaps.
        ///     <remarks>If <paramref name="useAtlas"/> is true this won't take effect.</remarks>
        /// </param>
        /// <param name="filteringMode">The texture filtering mode.
        ///     <remarks>If <paramref name="useAtlas"/> is true this won't take effect.</remarks>
        /// </param>
        /// <returns>The texture.</returns>
        Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT,
            bool useAtlas, bool? manualMipmaps = null, TextureFilteringMode? filteringMode = null);

        /// <summary>
        /// Retrieve a <see cref="SampleChannel"/>.
        /// </summary>
        /// <param name="audioInfo">The requested sample.</param>
        /// <returns>A matching sample channel, or null if unavailable.</returns>
        ISample GetSample(IAudioInfo audioInfo);

        /// <summary>
        /// Retrieve a <see cref="ITrack"/>.
        /// </summary>
        /// <param name="audioInfo">The requested track.</param>
        /// <returns>A matching track, or null if unavailable.</returns>
        ITrack GetTrack(IAudioInfo audioInfo);

        /// <summary>
        /// Retrieve an asset path.
        /// </summary>
        /// <param name="componentName">The requested asset.</param>
        /// <returns><paramref name="componentName"/> or a path which was overriden by an alias inside the <see cref="ResourcePack"/>.</returns>
        string GetPath(string componentName);
    }
}
