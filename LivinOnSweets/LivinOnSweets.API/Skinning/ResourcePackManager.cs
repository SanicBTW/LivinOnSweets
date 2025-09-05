using JetBrains.Annotations;
using LivinOnSweets.API.Audio;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.IO;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Tomlyn;
using Tomlyn.Syntax;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace LivinOnSweets.API.Skinning
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Skinning/SkinManager.cs
    /// <summary>
    /// Class that manages the available <see cref="ResourcePack"/>s
    /// </summary>
    public class ResourcePackManager : IStorageResourceProvider, IResourcePackSource
    {
        /// <summary>
        /// The oficial resource packs ids embedded within the game, in order.
        /// </summary>
        public static readonly string[] OFFICIAL_RESOURCE_PACKS = ["sugar_rush", "livin_on_sweets", "antique_seraphim", "extra_antique_seraphim"];

        private static readonly TomlModelOptions default_toml_options = new() { ConvertPropertyName = firstLowerPascal };

        private readonly AudioManager audio;

        private readonly GameHost host;

        private readonly IResourceStore<byte[]> resources;

        private readonly Storage storage;

        private readonly Dictionary<string, ResourcePack> loadedPacks = new();

        public IEnumerable<ResourcePack> LoadedPacks => loadedPacks.Values;

        [CanBeNull] public ResourcePack GetPackById(string id) =>
            loadedPacks.TryGetValue(id, out var pack) ? pack : null;

        private Bindable<string> resourcePack;
        private Bindable<GameUpdateVersion> gameUpdate;

        /// <summary>
        /// The current bound <see cref="ResourcePack"/>.
        /// </summary>
        public readonly Bindable<ResourcePack> CurrentPack = new();

        public readonly Logger Logger;

        public ResourcePackManager(Storage storage, GameHost host, ResourceStore<byte[]> resources,
            AudioManager audio, SweetConfigManager sweetConfig)
        {
            this.audio = audio;
            this.host = host;
            this.resources = resources;
            this.storage = storage.GetStorageForDirectory("resourcepacks");

            Logger = Logger.GetLogger("resources");
            Logger.Add("Resource Pack Manager instantiated", LogLevel.Debug);

            // It will follow a resource pack id, the way to retrieve the resource pack name should be by getting the pack then pack info, metadata and name
            resourcePack = sweetConfig.GetBindable<string>(SweetSetting.ResourcePack);
            gameUpdate = sweetConfig.GetBindable<GameUpdateVersion>(SweetSetting.GameUpdate);

            loadEmbedded();
            loadExternal();

            bool isOfficial = OFFICIAL_RESOURCE_PACKS.Contains(resourcePack.Value); // Checks if the set resource pack is coming from the official resources
            bool sameUpdatePack = gameUpdate.Value.GetDescription() == resourcePack.Value; // Checks if the set game update resource pack id is the same as the resource pack set

            // this edge case was found within the editor, when changing alone the resource pack it would need
            // to change the game update version too but since it doesnt the packs were mismatching thus crashing the game
            // this is a quick fix in case of forgetting to change the value
            bool isForcedPack = isOfficial && !sameUpdatePack; // Checks if the provided resource pack is a listed one AND the game update isnt equal (mismatch!)
            if (isForcedPack)
            {
                int packIndex = OFFICIAL_RESOURCE_PACKS.ToList().IndexOf(resourcePack.Value);
                GameUpdateVersion convVer = (GameUpdateVersion)packIndex;
                gameUpdate.Value = convVer;

                // should re-evaluate some flags
                sameUpdatePack = gameUpdate.Value.GetDescription() == resourcePack.Value;
                Logger.Add("Forced to reset the GameUpdate to match ResourcePack", LogLevel.Debug);
            }

            // Run immediately if: is not official OR its the same res pack id
            bool runImmediately = !isOfficial || sameUpdatePack;
            resourcePack.BindValueChanged(ev => updateCurrentPack(ev.NewValue), runImmediately);

            // This binds to any change the game update bindable has, doesnt run immediately since its
            // most likely to already have the same value in the resource pack bindable
            gameUpdate.BindValueChanged(ev =>
            {
                resourcePack.Value = ev.NewValue.GetDescription();
            });

            // Just like osu!lazer, only be able to change the current pack by other source
            // https://github.com/ppy/osu/blob/3cbdf2b890bf4573764afcd40e94391bc8fdb827/osu.Game/Skinning/SkinManager.cs#L116
            CurrentPack.ValueChanged += ev =>
            {
                if (ev.NewValue.PackInfo.Metadata.Id != resourcePack.Value)
                    throw new InvalidOperationException(
                        $"Setting {nameof(CurrentPack)}'s value directly is not supported. Change the {nameof(SweetSetting.ResourcePack)} bindable instead.");

                SourceChanged?.Invoke();
            };
        }

        private void loadEmbedded()
        {
            using NamespacedResourceStore<byte[]> res = new NamespacedResourceStore<byte[]>(resources, "ResourcePacks");

            // I could search for the .toml files inside the resources but for sanity I will use the set array
            foreach (string packId in OFFICIAL_RESOURCE_PACKS)
            {
                using Stream metaStream = res.GetStream($"{packId}/metadata.toml");
                if (metaStream == null)
                {
                    Logger.Add($"Failed to retrieve {packId}", LogLevel.Error);
                    continue;
                }

                using StreamReader metaReader = new StreamReader(metaStream);
                cachePack(metaReader);
            }
        }

        private void loadExternal()
        {
            // Will look for resource packs inside the user storage
            using StorageBackedResourceStore extResourcePacks = new StorageBackedResourceStore(storage);
            string[] extTomls = extResourcePacks.GetAvailableResources().Where(s => s.EndsWith(".toml")).ToArray();
            if (extTomls.Length == 0)
                return;

            foreach (string extToml in extTomls)
            {
                using Stream metaStream = extResourcePacks.GetStream(extToml);
                if (metaStream == null) // This shouldn't really happen since its like, looping through the files that it retrieved so its weird
                {
                    Logger.Add($"{extToml} returned a null stream.", LogLevel.Error);
                    continue;
                }

                using StreamReader metaReader = new StreamReader(metaStream);
                cachePack(metaReader);
            }
        }

        private void cachePack(StreamReader metaReader)
        {
            string metaContent = metaReader.ReadToEnd();
            if (!Toml.TryToModel(metaContent, out ResourcePackInfo packInfo,
                    out DiagnosticsBag diagnostics, options: default_toml_options))
            {
                Logger.Add("Failed while parsing the TOML model, please check the log file for more information",
                    LogLevel.Error);
                Logger.Add(diagnostics.ToString(), outputToListeners: false);
                return;
            }

            loadedPacks[packInfo.Metadata.Id] = new ResourcePack(packInfo, this);
            Logger.Add($"Cached {packInfo.Metadata.Id}");
        }

        private void updateCurrentPack(string newPackId)
        {
            Logger.Add($"Changing to {newPackId} resource pack");
            ResourcePack nextPack = GetPackById(newPackId);
            if (nextPack == null)
            {
                NullReferenceException exception =
                    new NullReferenceException($"Couldn't set {nameof(nextPack)} to {newPackId}");

                Logger.Add("Failed to change, nextPack was null", LogLevel.Error, exception);

                // Throwing here is safer since everything else is probably gonna throw too because they're accessing CurrentPack.Value
                throw exception;
            }

            // Null probably; it has the default value which is currently null
            if (CurrentPack.IsDefault)
            {
                Logger.Add("There's no current pack set, applying new pack instantly", LogLevel.Important);
                CurrentPack.Value = nextPack;
                return;
            }

            ResourcePack current = CurrentPack.Value;
            if (current == nextPack)
            {
                Logger.Add("Attempted to change to the same pack", LogLevel.Error);
                return;
            }

            if (!canChangePacks(current.PackInfo, nextPack.PackInfo))
                return;

            Logger.Add($"Successfully changed to {newPackId}", LogLevel.Important);
            CurrentPack.Value = nextPack;
        }

        // after a couple of iterations now this has changed, with the addition of reloadable sprites, the sprite itself
        // can get re-created upon pack change but this leads to some edge cases where you cant change the pack on runtime
        // avoiding some runtime performance issues maybe
        private static bool canChangePacks(ResourcePackInfo currentInfo, ResourcePackInfo nextInfo)
        {
            // TODO: Implement soft restart (restarting the game as a whole or re-create the screen stack, game instance, not the app to make it simpler)
            // Should only apply to the embedded resource packs, but I should also guard for edge cases coming from outside
            // Should account for external variables, for example:
            /*
             * The player is currently in the main screen, it should only need to refresh some objects, change the pack
             * The player is currently in game, reloading would only need to restart the created game instance
             * The player is in the middle of a song, the change should be scheduled (not likely to happen?)
             * Overall the manager should look for used textures inside the pack, dispose them then refresh the objects, which is already triggered through SourceChanged
             */
            if (!currentInfo.Engine.CompatibleWith.Contains(nextInfo.Metadata.Id))
            {
                Logger.Log("Cannot change to an incompatible pack, requires a restart", "resources", LogLevel.Error);
                return true; // Returning true for the time being since the sprite should handle the change of the resource pack but so does the game entirely
            }

            if (nextInfo.Engine.RequiresRestart)
            {
                Logger.Log("The next resource pack is asking for a restart", "resources");
                return false;
            }

            return true;
        }

        #region IResourceStorageProvider

        IRenderer IStorageResourceProvider.Renderer => host.Renderer;
        AudioManager IStorageResourceProvider.AudioManager => audio;
        IResourceStore<byte[]> IStorageResourceProvider.Resources => resources;
        Storage IStorageResourceProvider.Storage => storage;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);

        #endregion

        #region IResourcePackSource

        public event Action SourceChanged;

        public ResourcePackInfo PackInfo => CurrentPack.Value.PackInfo;

        // Quick wrappers but its only to satisfy the interface, it should implement lookups or fallbacks for the available skins
        // But that is managed by the resource pack itself...
        public virtual ISample GetSample(IAudioInfo audioInfo) => CurrentPack.Value.GetSample(audioInfo);

        public virtual ITrack GetTrack(IAudioInfo audioInfo) => CurrentPack.Value.GetTrack(audioInfo);

        // Will call the other get texture function which already looks for the alias
        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) =>
            GetTexture(componentName, wrapModeS, wrapModeT, true);

        public virtual Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT,
            bool useAtlas, bool? manualMipmaps = null, TextureFilteringMode? filteringMode = null) =>
            CurrentPack.Value.GetTexture(componentName, wrapModeS, wrapModeT, useAtlas, manualMipmaps, filteringMode);

        public string GetPath(string componentName) => CurrentPack.Value.GetPath(componentName);

        #endregion

        private static string firstLowerPascal(string s)
        {
            char first = s[0]; // Take the first letter, e.g: M
            string rest = s[1..]; // Take the rest of the string except the first letter, e.g: etadata
            return char.ToLowerInvariant(first) + rest; // Join the stuff: metadata
        }
    }
}
