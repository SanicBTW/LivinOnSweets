using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using AetherFramework;
using LivinOnSweets.API;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Cursor;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Localisation;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.Stores;
using LivinOnSweets.Resources;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGameBase : osu.Framework.Game
    {
        protected override Container<Drawable> Content => content;
        private Container content;

        protected ManiaActionContainer ActionContainer;

        protected DependencyContainer GameDependencies;

        // Copied from lazer lol
        protected SafeAreaContainer SafeAreaContainer { get; private set; }

        /// <summary>
        /// The <see cref="Edges"/> that the game should be drawn over at a top level.
        /// Defaults to <see cref="Edges.None"/>.
        /// </summary>
        protected virtual Edges SafeAreaOverrideEdges => Edges.None;

        protected Storage Storage { get; set; }

        protected SweetConfigManager SweetConfig { get; set; }

        protected SingleThreadLoad SingleThreadLoad { get; set; }

        protected ResourcePackManager ResourcePackManager { get; set; }

        // language bs
        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGameBase.cs#L171
        public Bindable<Language> CurrentLanguage { get; } = new();

        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGameBase.cs#L233C9-L235C82
        private Bindable<string> frameworkLocale = null!;

        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected LivinOnSweetsGameBase()
        {
            ClassRegistry.RegisterOverridableClasses(Assembly.GetExecutingAssembly());
            ClassRegistry.RegisterOverridableClasses(LivinOnSweetsApi.ApiAssembly);
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            // Used to share the "loadComponentSingleFile" function to the children of this game, without having to access this entirely
            base.Content.Add(SingleThreadLoad = new SingleThreadLoad());

            Resources.AddStore(new DllResourceStore(LivinOnSweetsResources.ResourceAssembly));
            SetupDependencies(GameDependencies);
            SetupSongStore(GameDependencies);
            SetupFonts();

            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            frameworkLocale.BindValueChanged(_ => updateLanguage());

            localisationParameters = Localisation.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateLanguage(), true);

            CurrentLanguage.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());

            base.Content.Add(SafeAreaContainer = new SafeAreaContainer
            {
                SafeAreaOverrideEdges = SafeAreaOverrideEdges,
                RelativeSizeAxes = Axes.Both,
                Child = CreateScalingContainer().WithChild(ActionContainer = new ManiaActionContainer().WithChild(content = new ModularCursorDisplay() { RelativeSizeAxes = Axes.Both }))
            });

            GameDependencies.CacheAs(ActionContainer);
            base.Content.Add(new TouchInputInterceptor());
        }

        protected virtual void SetupDependencies(DependencyContainer container)
        {
            // Used to save states and react to them on some parts of the game
            container.Cache(new GameStateManager());

            // Cache the storage variable from the host since it will be used inside the configuration managers
            // And make it accessible across the tree
            container.Cache(Storage);

            // Create a new texture upload from the game resources store
            IResourceStore<TextureUpload> texUpload = Host.CreateTextureLoaderStore(Resources);
            Textures.AddTextureSource(texUpload); // Add the newly created texture upload into the existing texture store

            // Create a new large texture store, probably the only stuff we need
            LargeTextureStore largeTs = new(Host.Renderer, texUpload);
            container.Cache(largeTs);

            container.Cache(new SessionConfig());
            container.Cache(SweetConfig);
            container.Cache(SingleThreadLoad);

            container.Cache(ResourcePackManager = new ResourcePackManager(Storage, Host, Resources, Audio, SweetConfig));
            container.CacheAs<IResourcePackSource>(ResourcePackManager);

            // for the future me uhhh the whole aether framework configuration workflow changes on 1.1.2 beta, so the osu configuration provider shouldnt work at all
            // that includes that both 1.1.2 beta and the osu framework extension nuget package are NOT published yet since they lack the decent amount of quality i want
            // also idk if i should keep a variable that references the loader but it will live in the dependencies so uhh yeah
            container.Cache(new ModLoader(config: new OsuFrameworkConfigurationProvider(Storage)));
        }

        protected virtual void SetupSongStore(DependencyContainer container)
        {
            SongStore songStore = new SongStore();

            // this code was provided by good 'ol me!! (kind of)
            Storage resxPackStorage = Storage.GetStorageForDirectory("resourcepacks");
            foreach (ResourcePack pack in ResourcePackManager.LoadedPacks)
            {
                Storage packStorage = resxPackStorage.GetStorageForDirectory(pack.PackInfo.Metadata.Id);
                // resource packs on the local res file dont need cache at all
                songStore.AddStore(new ResourcePackSongStore(pack, pack.IsLocalPack ? null : packStorage));
            }

            container.Cache(songStore);
        }

        protected virtual void SetupFonts()
        {
            AddFont(Resources, "Fonts/GyeonggiTitle/GyeonggiTitle");
            AddFont(Resources, "Fonts/GyeonggiTitle/GyeonggiTitle-Bold");

            AddFont(Resources, "Fonts/Prompt/Prompt");

            AddFont(Resources, "Fonts/DNFBitBit/DNFBitBit");
            AddFont(Resources, "Fonts/DNFBitBit/DNFBitBit-Italic");
        }

        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGameBase.cs#L424
        // Apparently there's like 4 calls on first run (locale is null on framework.ini)
        // After the first run its only 2 calls
        private void updateLanguage() => CurrentLanguage.Value = LanguageExtensions.GetLanguageFor(frameworkLocale.Value, localisationParameters.Value);

        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGame.cs#L905
        protected virtual void LoadLocales()
        {
            PreservingNamespaceResourceStore<byte[]> localeNamespace =
                new PreservingNamespaceResourceStore<byte[]>(Resources, "Localisation");

            Language[] languages = Enum.GetValues<Language>();

            IEnumerable<LocaleMapping> mappings = languages.Select(lang =>
            {
                string cultureCode = lang.ToCultureCode();

                try
                {
                    return new LocaleMapping(new TomlLocalisationStore(localeNamespace, cultureCode));
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to load localisations for language \"{cultureCode}\"");
                    return null;
                }
            }).Where(m => m != null);

            Localisation.AddLocaleMappings(mappings);
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            Storage ??= host.Storage;

            // first run might not have a config file, once the settings panel is closed it will trigger a save to create it
            // or any change to the settings will perform a save call, ill have to look into it
            SweetConfig = new SweetConfigManager(Storage);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            SweetConfig?.Dispose();
        }

        protected virtual Container CreateScalingContainer() => new DrawSizePreservingFillContainer();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
        {
            // Setting the default locale to en to populate the object on first run instead of having an emtpy string and falling back on localisable strings
            { FrameworkSetting.Locale, "en" },
            { FrameworkSetting.WindowedSize, new Size(1280, 720) }
        };
    }
}
