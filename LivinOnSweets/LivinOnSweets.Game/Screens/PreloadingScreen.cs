using System.Collections.Generic;
using System.Linq;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Screens;
using LivinOnSweets.Game.SubScreens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;

namespace LivinOnSweets.Game.Screens
{
    // First screen ever visible in game, it preloads the resources the game will need
    public partial class PreloadingScreen : SweetScreen
    {
        [Resolved]
        private SweetConfigManager config { get; set; }

        private SweetScreen nextScreen;
        private ShaderPrecompiler precompiler;

        private FillFlowContainer<PreloaderTextTracker> trackers;

        public PreloadingScreen()
        {
            ValidForResume = false;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            if (!config.Get<bool>(SweetSetting.SkipProjectDisclaimer))
                PushSubScreen(new ProjectDisclaimer());

            AddInternal(trackers = new FillFlowContainer<PreloaderTextTracker>()
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(12),
                AutoSizeDuration = 500F,
                AutoSizeEasing = Easing.OutQuint,
                LayoutDuration = 500F,
                LayoutEasing = Easing.OutQuint,
            });
            trackers.FadeTo(0.5F, 1000D, Easing.InOutQuart).Then().FadeTo(0.9F, 1000D, Easing.InOutQuart).Loop();

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);
            LoadComponentAsync(nextScreen = CreateNextScreen());

            trackers.Add(new PreloaderTextTracker(precompiler));
            trackers.Add(new PreloaderTextTracker(new ScreenPreloaderTracker(nextScreen)));

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (trackers.Count > 0 || IsSubScreenOpen)
            {
                Schedule(checkIfLoaded);
                return;
            }

            Push(nextScreen);
        }

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new();

        protected virtual SweetScreen CreateNextScreen() => new StartupScreen();

        public partial class PreloaderTextTracker : SpriteText
        {
            public static FontUsage DefaultFontUsage => new(family: "DNFBitBit", size: 16F, italics: true);

            private IPreloadable tracking;

            public PreloaderTextTracker(IPreloadable preloadable)
            {
                tracking = preloadable;

                Font = DefaultFontUsage;
                Anchor = Origin = Anchor.BottomRight;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Text = tracking.PreloadingText;
            }

            protected override void Update()
            {
                base.Update();

                // My idea was to slide the text out of the screen but uhhh shit happens
                // too complicated to keep the flow position then doing magic, nah
                if (!tracking.Preloaded) return;

                // Giving a little bit of grace time to look like it actually preloaded something??
                this.Delay(750D).FadeOut(250D, Easing.OutQuint).OnComplete(_ => Expire());
            }
        }

        public partial class ShaderPrecompiler : Component, IPreloadable
        {
            private readonly List<IShader> loadTargets = [];
            protected virtual bool AllLoaded => loadTargets.All(s => s.IsLoaded);

            [BackgroundDependencyLoader]
            private void load(ShaderManager manager)
            {
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"FastCircle"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"CircularProgress"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"SaturationSelectorBackground"));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, @"HueSelectorBackground"));

                // Should backtrack the Backdrop shader into using some tile shader instead of having the logic of tiling there
                // The backdrops purpose is to scroll the background
                loadTargets.Add(manager.Load(@"Backdrop", @"Backdrop"));
            }

            protected override void Update()
            {
                base.Update();

                if (!AllLoaded) return;
                Expire();
            }

            public string PreloadingText => "Preloading shaders...";
            public bool Preloaded => AllLoaded;
        }

        private partial class ScreenPreloaderTracker(SweetScreen nextScreen) : Component, IPreloadable
        {
            public string PreloadingText => $"Preloading {nextScreen.GetType().Name}...";
            public bool Preloaded => nextScreen.LoadState == LoadState.Ready;
        }

        // Will backtrack to the API, for now its just for testing the preloading screen
        // Easy interface that marks a component as preloadable, giving the necessary information
        // For the PreloadingScreen
        public interface IPreloadable
        {
            // The label to display on the bottom right part of the screen
            string PreloadingText { get; }

            // If the component has been preloaded
            bool Preloaded { get; }
        }
    }
}
