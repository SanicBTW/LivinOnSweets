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
        public readonly ResourcePackInfo PackInfo;

        private readonly ResourcePackManager resourcePackManager;

        private readonly ResourceStore<byte[]> store = new();

        /// <summary>
        /// A texture store which can be used to perform user file lookups for this resource pack.
        /// </summary>
        protected TextureStore Textures { get; }

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

            // The ID should be passed down but uhh I added it to the resource pack metadata, just to make the future a little bit brighter
            // Namespace formed like: ResourcePack/id/... not like id/... dumb ass - to myself sanco
            NamespacedResourceStore<byte[]> packResources = new NamespacedResourceStore<byte[]>(resources.Resources, $"ResourcePacks/{PackInfo.Metadata.Id}");
            store.AddStore(packResources);

            // both audio stores will share the same store, adding any other store to this will affect the backing audio store
            ISampleStore samples = resources.AudioManager.GetSampleStore(store);
            samples.AddExtension("ogg");

            Samples = samples;
            Tracks = resources.AudioManager.GetTrackStore(store);

            // The reason why we use pack resources and not bind the store itself, its because we want the texture lookup to fail
            // so it fallbacks to the nested stores, reusing some texture cache and texture atlases from them
            Textures = new TextureStore(resources.Renderer, resources.CreateTextureLoaderStore(packResources));

            if (string.IsNullOrWhiteSpace(PackInfo.Metadata.Fallback)) return;
            addFallback();
        }

        private void addFallback()
        {
            // Instead of creating a whole new store and shi, we retrieve the pack from the manager to add their stores to this pack
            ResourcePack fallbackPack = resourcePackManager.GetPackById(PackInfo.Metadata.Fallback);
            if (fallbackPack == null)
            {
                Logger.Log($"Failed to retrieve the fallback {PackInfo.Metadata.Fallback}", "resources", LogLevel.Error);
                return;
            }

            store.AddStore(fallbackPack.store);
            Textures.AddStore(fallbackPack.Textures); // This is done to avoid having atlases per each resource pack and properly retrieving cache
            Logger.Log($"Mapped {PackInfo.Metadata.Id} stores to fallback to {PackInfo.Metadata.Fallback} stores", "resources", LogLevel.Debug);
        }

        #region IResourcePack

        public virtual ISample GetSample(IAudioInfo audioInfo) =>
            audioInfo.LookupNames.Select(lookup => Samples?.Get(GetPath(lookup))).FirstOrDefault(sample => sample != null);

        public virtual ITrack GetTrack(IAudioInfo audioInfo) =>
            audioInfo.LookupNames.Select(lookup => Tracks?.Get(GetPath(lookup))).FirstOrDefault(sample => sample != null);

        // Will call the other get texture function which already looks for the alias
        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public virtual Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) =>
            Textures.Get(GetPath(componentName), wrapModeS, wrapModeT);

        // Will look for aliases inside the table, if none it will return the given argument
        // In reality, aliases are just a sweetened way of overriding
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
