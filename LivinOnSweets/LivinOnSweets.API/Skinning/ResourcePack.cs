using JetBrains.Annotations;
using LivinOnSweets.API.Audio;
using LivinOnSweets.API.IO;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;

namespace LivinOnSweets.API.Skinning
{
    // Kinda based on the Skin system of osu!lazer, configuration not implemented since there's really no config at all
    // Except the other tables which define stuff completely irrelevant to the real asset retrieval
    // But in favor of a more flexible system it should be definitely be added or at this point, let scripts modify the object
    /// <summary>
    /// Class that manages the asset retrieval as well as positioning of some game objects.
    /// </summary>
    public class ResourcePack : IDisposable, IResourcePack
    {
        public ResourcePackInfo PackInfo { get; protected set; }

        [CanBeNull] public ResourcePack Fallback { get; protected set; }

        private readonly ResourcePackManager resourcePackManager;

        private readonly ResourceStore<byte[]> store = new();

        /// <summary>
        /// A texture store which can be used to perform user file lookups for this resource pack.
        /// </summary>
        protected ResourcePackTextureStore Textures { get; }

        /// <summary>
        /// A sample store which can be used to perform user file lookups for this resource pack.
        /// </summary>
        protected ISampleStore Samples { get; }

        /// <summary>
        /// A track store which can be used to perform user file lookups for this resource pack.
        /// </summary>
        protected ITrackStore Tracks { get; }

        /// <summary>
        /// Construct a new skin.
        /// </summary>
        /// <param name="resourcePack">The resource pack metadata.</param>
        /// <param name="resourcePackManager">Access to the resource pack manager.
        ///     <remarks>Will be casted into <see cref="IStorageResourceProvider"/> for access to game-wide resources.</remarks>
        /// </param>
        public ResourcePack(ResourcePackInfo resourcePack, ResourcePackManager resourcePackManager)
        {
            this.resourcePackManager = resourcePackManager;

            IStorageResourceProvider resources = resourcePackManager;
            if (resources == null)
                throw new NullReferenceException();

            PackInfo = resourcePack;

            IResourceStore<byte[]> packResources = RetrievePackResources();
            store.AddStore(packResources);

            // both audio stores will share the same store, adding any other store to this will affect the backing audio store
            ISampleStore samples = resources.AudioManager.GetSampleStore(store);
            samples.AddExtension("ogg");

            Samples = samples;
            Tracks = resources.AudioManager.GetTrackStore(store);

            // The reason why we use pack resources and not bind the store itself, its because we want the texture lookup to fail
            // so it fallbacks to the nested stores, reusing some texture cache and texture atlases from them
            Textures = new ResourcePackTextureStore(resources.Renderer, resources.CreateTextureLoaderStore(packResources));

            if (string.IsNullOrWhiteSpace(PackInfo.Metadata.Fallback)) return;
            addFallback();
        }

        private void addFallback()
        {
            // Instead of creating a whole new store and shi, we retrieve the pack from the manager to add their stores to this pack
            Fallback = resourcePackManager.GetPackById(PackInfo.Metadata.Fallback);
            if (Fallback == null)
            {
                Logger.Log($"Failed to retrieve the fallback {PackInfo.Metadata.Fallback}", "resources", LogLevel.Error);
                return;
            }

            store.AddStore(Fallback.store);
            Textures.AddStore(Fallback.Textures); // This is done to avoid having atlases per each resource pack and properly retrieving cache
            resourcePackManager.Logger.Add($"Mapped {PackInfo.Metadata.Id} stores to fallback to {PackInfo.Metadata.Fallback} stores");
            ApplyFallbackMetadata();
        }

        // you could say its sanitizing the data rather than applying a fallback but uh yeah, this is bad practice i should move the sanitization to another method
        /// <summary>
        /// Applies the fallback metadata to this <see cref="ResourcePack"/>
        /// </summary>
        protected virtual void ApplyFallbackMetadata()
        {
            resourcePackManager.Logger.Add($"Applying {PackInfo.Metadata.Fallback} metadata to the current pack ({PackInfo.Metadata.Id})");

            ResourcePackInfo fallbackInfo = Fallback!.PackInfo;

            // i honestly dont know if we should inherit aliases ehh

            if (PackInfo.Songs == null)
            {
                // in the best cases we should only need to inherit the fallback entirely
                // cases like livin on sweets using the same songs as sugar rush
                // for extra antique seraphim this is different since it ADDS songs to the fallback
                PackInfo.Songs = fallbackInfo.Songs;
            }
            else // we need to sanitize some
            {
                // check extra antique seraphim song table
                if (PackInfo.Songs.Format == "")
                    PackInfo.Songs.Format = fallbackInfo.Songs.Format;

                PackInfo.Songs.Separator ??= fallbackInfo.Songs.Separator;

                // here we need to check if the fallback pack IS compatible with the current pack
                // this is because livin on sweets and before do not need multilist, however antique seraphim and newer
                // need multilist, extra antique seraphim inherits from antique seraphim soo
                bool isPackCompatible = PackInfo.Engine.CompatibleWith.Contains(fallbackInfo.Metadata.Id);
                if (isPackCompatible && PackInfo.Songs.MultiList != fallbackInfo.Songs.MultiList)
                    PackInfo.Songs.MultiList = fallbackInfo.Songs.MultiList;

                List<string> fallbackSongs = fallbackInfo.Songs.Available;

                // ["...", "twinkle_magic"] returns 0
                int indexOfSpread = PackInfo.Songs.Available.IndexOf("...");
                if (indexOfSpread == -1)
                    return;

                // ["twinkle_magic"]
                PackInfo.Songs.Available.RemoveAt(indexOfSpread);

                // ["tomodachi_onestep", "tremendous_celebration", "twinkle_magic"] best approach
                PackInfo.Songs.Available.InsertRange(indexOfSpread, fallbackSongs);
            }

        }

        public IResourceStore<byte[]> RetrievePackResources()
        {
            // should make some function to assert the convertion and retrieval
            IStorageResourceProvider resources = resourcePackManager;
            if (resources == null)
                throw new NullReferenceException();

            string packNamespace = $"ResourcePacks/{PackInfo.Metadata.Id}";
            IResourceStore<byte[]> packResources;
            bool isInResources = resources.Resources.GetAvailableResources().Any(str => str.StartsWith(packNamespace)); // bruh
            if (isInResources)
            {
                // The ID should be passed down but uhh I added it to the resource pack metadata, just to make the future a little bit brighter
                // Namespace formed like: ResourcePacks/id/... not like id/... dumb ass - to myself sanco
                packResources = new NamespacedResourceStore<byte[]>(resources.Resources, $"ResourcePacks/{PackInfo.Metadata.Id}");
            }
            else
                packResources = new StorageBackedResourceStore(resources.Storage.GetStorageForDirectory(PackInfo.Metadata.Id));

            return packResources;
        }

        #region IResourcePack

        public virtual ISample GetSample(IAudioInfo audioInfo) =>
            audioInfo.LookupNames.Select(lookup => Samples?.Get(GetPath(lookup))).FirstOrDefault(sample => sample != null);

        public virtual ITrack GetTrack(IAudioInfo audioInfo) =>
            audioInfo.LookupNames.Select(lookup => Tracks?.Get(GetPath(lookup))).FirstOrDefault(sample => sample != null);

        // Will call the other get texture function which already looks for the alias
        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) =>
            GetTexture(componentName, wrapModeS, wrapModeT, true);

        public virtual Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT,
            bool useAtlas, bool? manualMipmaps = null, TextureFilteringMode? filteringMode = null) =>
            Textures.Get(componentName, wrapModeS, wrapModeT, useAtlas, manualMipmaps, filteringMode);

        // Will look for aliases inside the table, if none it will return the given argument
        // In reality, aliases are just a sweetened way of overriding paths without minding the real file structure
        // so as long as you know the path call you can change it
        public string GetPath(string componentName)
        {
            Dictionary<string, string> aliases = PackInfo.Aliases;
            if (aliases.Count == 0)
                return componentName; // No aliases available, just return the argument

            return aliases.TryGetValue(componentName, out string overridenPath)
                ? overridenPath
                : componentName;
        }

        #endregion

        #region Disposal

        ~ResourcePack()
        {
            // required to potentially clean up sample store from audio hierarchy.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            Textures?.Dispose();
            Samples?.Dispose();
            Tracks?.Dispose();

            store.Dispose();
        }

        #endregion
    }
}
