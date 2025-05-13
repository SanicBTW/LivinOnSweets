using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;

namespace LivinOnSweets.API.Skinning
{
    // me from the future: i thought about using 4096 atlases only for veldrid renderer but with mipmapping added,
    // it was using around 400mb of memory, not really ideal if it only has 5 images inside, i could disable mipmapping overall in the atlas but... shi lame
    /// <summary>
    /// A custom implementation of <see cref="TextureStore"/> for <see cref="ResourcePack"/>'s avoiding
    /// multiple stores to change <see cref="TextureFilteringMode"/> or if it should use the <see cref="TextureAtlas"/> bound to it.
    /// </summary>
    public class ResourcePackTextureStore : ITextureStore
    {
        private readonly Dictionary<string, Texture> textureCache = [];
        private readonly Dictionary<string, Task> retrievalCompletionSources = [];

        private readonly ResourceStore<TextureUpload> uploadStore = new();
        private readonly List<ITextureStore> nestedStores = [];

        private readonly IRenderer renderer;

        private readonly TextureFilteringMode atlasFilteringMode;
        private readonly bool atlasManualMipmaps;
        private const int max_atlas_size = 1024;
        protected TextureAtlas Atlas;

        public readonly float ScaleAdjust;

        public ResourcePackTextureStore(IRenderer renderer, IResourceStore<TextureUpload> store = null, TextureFilteringMode atlasFilteringMode = TextureFilteringMode.Linear,
            bool atlasManualMipmaps = false, float scaleAdjust = 2)
        {
            if (store != null)
                AddTextureSource(store);

            this.renderer = renderer;
            this.atlasFilteringMode = atlasFilteringMode;
            this.atlasManualMipmaps = atlasManualMipmaps;

            ScaleAdjust = scaleAdjust;

            int size = Math.Min(max_atlas_size, renderer.MaxTextureSize);
            Atlas = new TextureAtlas(renderer, size, size, atlasManualMipmaps, atlasFilteringMode);
        }

        /// <inheritdoc cref="TextureStore.AddTextureSource(IResourceStore{TextureUpload})"/>
        public virtual void AddTextureSource(IResourceStore<TextureUpload> store) => uploadStore.AddStore(store);

        /// <inheritdoc cref="TextureStore.RemoveTextureStore(IResourceStore{TextureUpload})"/>
        public virtual void RemoveTextureStore(IResourceStore<TextureUpload> store) => uploadStore.RemoveStore(store);

        /// <inheritdoc cref="TextureStore.AddStore(ITextureStore)"/>
        public virtual void AddStore(ITextureStore store)
        {
            lock (nestedStores)
                nestedStores.Add(store);
        }

        /// <inheritdoc cref="TextureStore.RemoveStore(ITextureStore)"/>
        public virtual void RemoveStore(ITextureStore store)
        {
            lock (nestedStores)
                nestedStores.Remove(store);
        }

        private Texture loadRaw(TextureUpload upload, bool useAtlas, bool manualMipmaps, TextureFilteringMode filteringMode,
            WrapMode wrapModeS = WrapMode.None, WrapMode wrapModeT = WrapMode.None)
        {
            if (upload == null) return null;

            Texture texture = null;

            if (useAtlas && (texture = Atlas.Add(upload.Width, upload.Height, wrapModeS, wrapModeT)) == null)
            {
                Logger.Log($"Texture requested ({upload.Width}x{upload.Height}) which exceeds {nameof(ResourcePackTextureStore)}'s atlas size ({max_atlas_size}x{max_atlas_size}) - bypassing atlasing. Consider using {nameof(LargeTextureStore)}.",
                    LoggingTarget.Performance);
            }

            texture ??= renderer.CreateTexture(upload.Width, upload.Height, manualMipmaps, filteringMode, wrapModeS,
                wrapModeT);
            texture.ScaleAdjust = ScaleAdjust;
            texture.SetData(upload);

            return texture;
        }

        /// <inheritdoc cref="TextureStore.GetAsync(string, CancellationToken)"/>
        public Task<Texture> GetAsync(string name, CancellationToken cancellationToken) => GetAsync(name, default, default, cancellationToken);

        /// <inheritdoc cref="TextureStore.GetAsync(string, WrapMode, WrapMode, CancellationToken)"/>
        public Task<Texture> GetAsync(string name, WrapMode wrapModeT, WrapMode wrapModeS, CancellationToken cancellationToken = default) =>
            Task.Run(() => Get(name, wrapModeS, wrapModeT), cancellationToken);

        /// <inheritdoc cref="TextureStore.Get(string)"/>
        public Texture Get(string name) => Get(name, default, default);

        /// <inheritdoc cref="TextureStore.Get(string, WrapMode, WrapMode)"/>
        public virtual Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => Get(name, wrapModeS, wrapModeT, true);

        /// <summary>
        /// Retrieves a texture from the store and adds it to the atlas.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
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
        public virtual Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT,
            bool useAtlas, bool? manualMipmaps = null, TextureFilteringMode? filteringMode = null)
        {
            var texture = get(name, wrapModeS, wrapModeT, useAtlas, manualMipmaps, filteringMode);

            if (texture != null) return texture;
            lock (nestedStores)
            {
                foreach (var nested in nestedStores)
                {
                    if ((texture = nested.Get(name, wrapModeS, wrapModeT)) != null)
                        break;
                }
            }

            return texture;
        }

        public Stream GetStream(string name)
        {
            var stream = uploadStore.GetStream(name);

            if (stream != null) return stream;
            lock (nestedStores)
            {
                foreach (var nested in nestedStores)
                {
                    if ((stream = nested.GetStream(name)) != null)
                        break;
                }
            }

            return stream;
        }

        public IEnumerable<string> GetAvailableResources()
        {
            lock (nestedStores)
                return uploadStore.GetAvailableResources().Concat(nestedStores.SelectMany(s => s.GetAvailableResources()).ExcludeSystemFileNames()).ToArray();
        }

        private Texture get(string name, WrapMode wrapModeS, WrapMode wrapModeT, bool useAtlas, bool? manualMipmaps = null, TextureFilteringMode? filteringMode = null)
        {
            if (string.IsNullOrEmpty(name)) return null;

            manualMipmaps ??= atlasManualMipmaps;
            filteringMode ??= atlasFilteringMode;

            string key = $"{name}:wrap-{(int)wrapModeS}-{(int)wrapModeT}-{(int)filteringMode!}";

            TaskCompletionSource<Texture> tcs = null;
            Task task;

            lock (retrievalCompletionSources)
            {
                // Check if the texture exists in the cache.
                if (TryGetCached(key, out var cached))
                    return cached;

                // check if an existing lookup was already started for this key.
                if (!retrievalCompletionSources.TryGetValue(key, out task))
                    // if not, take responsibility for the lookup.
                    retrievalCompletionSources[key] = (tcs = new TaskCompletionSource<Texture>()).Task;
            }

            // handle the case where a lookup is already in progress.
            if (task != null)
            {
                task.WaitSafely();

                // always perform re-lookups through TryGetCached (see LargeTextureStore which has a custom implementation of this where it matters).
                return TryGetCached(key, out var cached) ? cached : null;
            }

            // this.LogIfNonBackgroundThread(key);

            Texture tex = null;

            try
            {
                tex = loadRaw(uploadStore.Get(name), useAtlas, manualMipmaps.Value, filteringMode.Value, wrapModeS, wrapModeT);
                if (tex != null)
                    tex.AssetName = key;

                return CacheAndReturnTexture(key, tex);
            }
            catch (TextureTooLargeForGLException)
            {
                Logger.Log($"Texture \"{name}\" exceeds the maximum size supported by this device ({renderer.MaxTextureSize}px).", level: LogLevel.Error);
            }
            finally
            {
                // notify other lookups waiting on the same name lookup.
                lock (retrievalCompletionSources)
                {
                    Debug.Assert(tcs != null);

                    tcs.SetResult(tex);
                    retrievalCompletionSources.Remove(key);
                }
            }

            return null;
        }

        /// <inheritdoc cref="TextureStore.TryGetCached(string, out Texture)"/>
        protected virtual bool TryGetCached([NotNull] string lookupKey, [CanBeNull] out Texture texture)
        {
            lock (textureCache)
                return textureCache.TryGetValue(lookupKey, out texture);
        }

        /// <inheritdoc cref="TextureStore.CacheAndReturnTexture(string, Texture)"/>
        [CanBeNull]
        protected virtual Texture CacheAndReturnTexture([NotNull] string lookupKey, [CanBeNull] Texture texture)
        {
            lock (textureCache)
                return textureCache[lookupKey] = texture;
        }

        /// <inheritdoc cref="TextureStore.Purge(Texture)"/>
        protected void Purge(Texture texture)
        {
            lock (textureCache)
            {
                if (textureCache.TryGetValue(texture.AssetName, out var tex))
                {
                    // we are doing this locally as right now, Textures don't dispose the underlying texture (leaving it to GC finalizers).
                    // in the case of a purge operation we are pretty sure this is the intended behaviour.
                    if (tex != null)
                        new DisposableTexture(tex).Dispose();
                }

                textureCache.Remove(texture.AssetName);
            }
        }

        #region IDisposable Support

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                uploadStore.Dispose();
                lock (nestedStores) nestedStores.ForEach(s => s.Dispose());
            }
        }

        #endregion
    }
}
