using System;
using System.Collections.Generic;
using System.Linq;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Localisation;
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
using osuTK;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGameBase : osu.Framework.Game
    {
        protected override Container<Drawable> Content { get; }
        private DependencyContainer gameDependencies;

        // language bs
        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGameBase.cs#L171
        public Bindable<Language> CurrentLanguage { get; } = new();

        // https://github.com/ppy/osu/blob/master/osu.Game/OsuGameBase.cs#L233C9-L235C82
        private Bindable<string> frameworkLocale = null!;

        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected LivinOnSweetsGameBase()
        {
            base.Content.Add(Content = new DrawSizePreservingFillContainer
            {
                TargetDrawSize = new Vector2(1280, 720)
            });
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            Resources.AddStore(new DllResourceStore(LivinOnSweetsResources.ResourceAssembly));
            SetupDependencies(gameDependencies);
            SetupSongStore(gameDependencies);
            SetupFonts();

            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            frameworkLocale.BindValueChanged(_ => updateLanguage());

            localisationParameters = Localisation.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateLanguage(), true);

            CurrentLanguage.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());
        }

        protected virtual void SetupDependencies(DependencyContainer container)
        {
            // Cache the storage variable from the host since it will be used inside the configuration managers
            // And make it accessible across the tree
            container.CacheAs(Host.Storage);

            // Used to save states and react to them on some parts of the game
            container.CacheAs(new GameStateManager());

            // Used to pass an accent store through the dp container
            container.CacheAs(new AccentStore(Resources));

            // Used to pass down the accents and populate those who implement the target interface
            AccentComponent component = new AccentComponent();
            container.CacheAs(component);
            Add(component);

            // Used to pass the main menu resources across the dp container
            container.CacheAs(new MainMenuStore(Host.Renderer, Resources));

            // used to pass the rhythm game resources across the dp container
            container.CacheAs(new RhythmGameStore(Host.Renderer, Resources, Audio));

            IResourceStore<TextureUpload> texUpload = Host.CreateTextureLoaderStore(Resources);

            LargeTextureStore largeTs = new(Host.Renderer, texUpload);
            container.CacheAs(largeTs);

            // since the game is pixel art (most of the times except the story mode sprites) we make a pixel art store to set the filter mode to nearest
            // Now the store is an animated one but can be fetched as a standard one or an animated one
            AnimatedPixelArtTextureStore pixArtTs = new(Host.Renderer, texUpload);
            container.CacheAs(typeof(PixelArtTextureStore), pixArtTs); // Cache as the derivative of PixelArtTextureStore
            container.CacheAs(pixArtTs); // Cache as AnimatedPixelArtTextureStore

            Action<IResourceStore<TextureUpload>>[] texLookups = [Textures.AddTextureSource, largeTs.AddTextureSource, pixArtTs.AddTextureSource];

            // Add the resource stores to the texture lookups
            container.CacheAs(AddToTextureLookup(new StoryModeStore(Resources), texLookups));
            container.CacheAs(AddToTextureLookup(new StartupStore(Resources), texLookups));

            // Add the namespaces to the texture lookups, in case the target store doesnt meet the needs of the moment
            AddToTextureLookup(new MainMenuNamespace(Resources), texLookups);
            AddToTextureLookup(new RhythmGameNamespace(Resources), texLookups);

            // Load up the action container
            ManiaActionContainer actionContainer = [];
            container.CacheAs(actionContainer);
            Content.Add(actionContainer);
        }

        protected virtual void SetupSongStore(DependencyContainer container)
        {
            SongStore songStore = new SongStore(Host.CacheStorage, Audio);
            songStore.AddStore(new LocalSongStore(Resources));
            // Search for converters or song stores for modular imports, should be done inside song store
            // loading assemblies for sure, preloading will happen on the PreloadScreen
            container.CacheAs(songStore);
        }

        protected virtual void SetupFonts()
        {
            AddFont(Resources, "Fonts/GyeonggiTitle/GyeonggiTitle");
            AddFont(Resources, "Fonts/GyeonggiTitle/GyeonggiTitle-Bold");
            AddFont(Resources, "Fonts/Prompt/Prompt");
            AddFont(Resources, "Fonts/DNFBitBit/DNFBitBit");
            AddFont(Resources, "Fonts/DNFBitBit/DNFBitBit-Italic");
        }

        // this might be insecure af but we balling with it anyways trust
        protected virtual T AddToTextureLookup<T>(T store, Action<IResourceStore<TextureUpload>>[] addFuncs)
            where T : IResourceStore<byte[]>
        {
            IResourceStore<TextureUpload> upload = Host.CreateTextureLoaderStore(store);

            foreach (Action<IResourceStore<TextureUpload>> addFunc in addFuncs)
                addFunc(upload);

            return store;
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

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            gameDependencies = new(base.CreateChildDependencies(parent));
    }
}
