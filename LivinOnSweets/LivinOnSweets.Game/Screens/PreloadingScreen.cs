using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.Game.SubScreens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Screens
{
    // First screen ever visible in game, it preloads the resources the game will need
    public partial class PreloadingScreen : SweetScreen
    {
        private SweetScreen nextScreen;
        private ShaderPrecompiler precompiler;

        [CanBeNull] private Box overlay; // quick overlay to hide the content loading behind
        private FillFlowContainer<PreloaderTextTracker> trackers;
        [CanBeNull] private ProjectDisclaimer disclaimer;

        public PreloadingScreen()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(SweetConfigManager config, IResourcePackSource pack)
        {
            bool skipDisclaimer = config.Get<bool>(SweetSetting.SkipProjectDisclaimer);
            if (!skipDisclaimer)
            {
                AddRangeInternal([
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("#e1ddd7")
                    },
                    // the reason why we do this instead of just filling the entire screen with the texture its because it
                    // gets stretched on both axes lookin kinda weird, fill looks fine? kinda what im looking for (no stretch, just filling the screen)
                    new Sprite
                    {
                        Texture = pack.GetTexture("Startup/UI/RewardsBG.png", WrapMode.None, WrapMode.None, false, true),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = Vector2.One,
                        FillMode = FillMode.Fill,
                    },
                    disclaimer = new ProjectDisclaimer(),
                    overlay = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                ]);
            }

            AddInternal(trackers = new FillFlowContainer<PreloaderTextTracker>
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
                Colour = skipDisclaimer ? Colour4.White : Colour4.Black
            });
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            overlay?.FadeOut(500D, Easing.OutQuint);

            // run the fade!
            trackers.FadeTo(0.5F, 1000D, Easing.InOutQuart).Then().FadeTo(0.9F, 1000D, Easing.InOutQuart).Loop();
            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);
            LoadComponentAsync(nextScreen = CreateNextScreen());

            trackers.Add(new PreloaderTextTracker(precompiler));
            trackers.Add(new PreloaderTextTracker(new ScreenPreloaderTracker(nextScreen)));

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (trackers.Count > 0 || disclaimer is { IsPresent: true })
            {
                Schedule(checkIfLoaded);
                return;
            }

            Push(nextScreen);
        }

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new();

        protected virtual SweetScreen CreateNextScreen() => new ReStartupScreen();

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

            // rarely crashes
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
