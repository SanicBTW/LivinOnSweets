using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Graphics.Sprites.EmbedPreload;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osuTK;
using osuTK.Input;

namespace LivinOnSweets.Game.Embedded
{
    // its the old code really, only some slight adjustments
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.Game/StartScreens/GameLoadScreen.cs
    public partial class EmbedPreloadScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        private LoadManager loadManager;
        private SweetScreen nextScreen;

        private SpriteText loadText;
        private bool loopin;

        private Box fadeOverlay;
        private bool transitioning;

        public EmbedPreloadScreen()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack, SweetConfigManager sweetConfig)
        {
            // kind of lame but aight
            bool isAntique = sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate) >=
                                GameUpdateVersion.AntiqueSeraphim;

            // Kind of a hacky way if you ask me, this represents the "music controls" visible in the background
            // it mimics its layout size, only to position the "loading bar" (only the handle sprite) on the proper
            // position of the background, i could use hardcoded positions but im full of "what ifs"
            MarginPadding musOverlayPadding = new MarginPadding() { Left = 60, Bottom = 52 }; // i didnt want to use a fixed position so i moved to margin (quick addition from the old code)
            Container bgMusicOverlay = new Container()
            {
                Size = new Vector2(616, 182),
                Anchor = isAntique ? Anchor.Centre : Anchor.BottomLeft,
                Origin = isAntique ? Anchor.Centre : Anchor.BottomLeft,
                Margin = isAntique ? new MarginPadding() : musOverlayPadding,
                Child = new Container()
                {
                    Size = new Vector2(616, 98),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children =
                    [
                        new LoadingBar(),
                        loadText = new SpriteText()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "NOW LOADING...",
                            // More accurate position and size relative to the OG version, the letters are a bit offseted on that version
                            Font = new FontUsage(family: "DNFBitBit", size: 58F),
                            Margin = new MarginPadding() { Right = 8, Bottom = 10 }
                        }
                    ]
                }
            };

            Texture bgTex = pack.GetTexture("GamePreload/Background", default, default, false, filteringMode: TextureFilteringMode.Nearest);
            bgTex.ScaleAdjust = 1;

            Texture logoTex = pack.GetTexture("GamePreload/Logo", default, default, false, filteringMode: TextureFilteringMode.Nearest);
            logoTex.ScaleAdjust = 1;

            Container musOverlay = isAntique ? new Container()
                {
                    AutoSizeAxes = Axes.Both, // it doesnt take the update notice sprite into account but i guess its better this way
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = musOverlayPadding,
                    Child = bgMusicOverlay,
                }
                : bgMusicOverlay;
            if (isAntique)
            {
                // referenced as update ribbon in the js file
                Texture updTex = pack.GetTexture("GamePreload/UpdateRibbon", default, default, false, filteringMode: TextureFilteringMode.Nearest);
                updTex.ScaleAdjust = 1;
                musOverlay.Add(new Sprite()
                {
                    Texture = updTex,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre,
                    Rotation = 15,
                    Margin = new MarginPadding() { Right = 136, Top = 2 },
                });
            }

            // This is made to properly position Airi with the background
            // Since the container "resizes", Airi is anchored to the Centre Right of her hitbox, which anchors to the
            // right part of the screen and when resized it wouldnt look correctly
            InternalChild = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Children =
                [
                    loadManager,
                    new Sprite()
                    {
                        Texture = bgTex,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new Sprite()
                    {
                        Texture = logoTex,
                        Margin = new MarginPadding { Top = 86, Left = 60 }, // first time baby woohoo - nvm
                    },
                    new AiriVibe(),
                    musOverlay,
                    fadeOverlay = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0f,
                        Colour = Colour4.Black
                    }
                ]
            };
        }

        protected override void Update()
        {
            if (loadManager.Progress < 1 || loopin || nextScreen is not { LoadState: LoadState.Ready })
                return;

            loopin = true;
            loadText.Text = "TOUCH TO START";
            loadText.Loop(t => t.FadeOut(800, Easing.InOutQuart).Then().FadeIn(800, Easing.InOutQuart));
        }

        // load the screen as soon as we enter
        public override void OnEntering(ScreenTransitionEvent e)
        {
            // delay the load a lil bit
            Scheduler.AddDelayed(() =>
            {
                LoadComponentAsync(nextScreen = CreateNextScreen());
            }, 500);
        }

        private bool handleInput()
        {
            if (transitioning || nextScreen is not { LoadState: LoadState.Ready })
                return false;

            transitioning = true;
            fadeOverlay.FadeInFromZero(1000D, Easing.OutQuint).OnComplete(_ => ScreenStack.Push(nextScreen));
            return true;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
            => e.Button == MouseButton.Left && handleInput();

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
            => e.Action == ManiaAction.CONFIRM && handleInput();

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        protected virtual SweetScreen CreateNextScreen() => new MainMenuScreen();

        // i have to manually register the dependency since for some reason it wont let me cache it through the attribute
        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.Cache(loadManager = new LoadManager());
            return dependencies;
        }

        protected partial class LoadingBar : Container
        {
            [Resolved] private LoadManager loadManager { get; set; }

            private Sprite handle;

            [BackgroundDependencyLoader]
            private void load(IResourcePackSource pack)
            {
                AutoSizeAxes = Axes.Y;
                Anchor = Origin = Anchor.BottomCentre;
                Margin = new MarginPadding { Right = 12 }; // kind of a forced value but can i do anything else?
                Width = 512;

                Texture texture = pack.GetTexture("GamePreload/LoadingHandle", default, default, false, filteringMode: TextureFilteringMode.Nearest);
                texture.ScaleAdjust = 1;
                Add(handle = new Sprite()
                {
                    Texture = texture,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                });
            }

            protected override void UpdateAfterChildren()
            {
                if (float.IsNaN(loadManager.Progress))
                    return;

                // woah
                float target = loadManager.Progress * (Width - handle.DrawWidth);
                float elapsed = (float)Time.Elapsed / 1000;
                const float smoothing = 5;
                float t = 1 - MathF.Exp(-smoothing * elapsed);
                handle.X = (float)Interpolation.Lerp(handle.X, target, t);
            }
        }

    }
}
