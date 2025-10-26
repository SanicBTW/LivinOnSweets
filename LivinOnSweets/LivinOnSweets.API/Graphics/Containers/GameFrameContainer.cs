using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Graphics.Sprites.Startup;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.StateMachines;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.API.Graphics.Containers
{
    // Old code mixed with improvements and more
    /// <summary>
    /// A container which manages the "game" context with its visual aspects
    /// </summary>
    public partial class GameFrameContainer(Action onGameEnter, bool hideBannersOnLoad = false)
        : ResourcePackReloadableDrawable, IHasContextMenu
    {
        [Resolved] private SweetConfigManager sweetConfig { get; set; }
        [Resolved] private GameSession gameSession { get; set; }

        public override bool DragBlocksClick => true; // block the click when dragging

        private int gameUpdate;

        private Container content;
        private FillFlowContainer flowContainer;

        private StudentPool students;
        private Sprite frame;
        // this is such a horrible hack omg
        // since the frame container now has a fill flow container for the banners n shit
        // now when showing the banners the frame shifts a little bit, so we use a margin which doubles that amount
        // to reshift the container while doing the sliding on the banners, not my favorite trick but looks good
        private readonly BindableMarginPadding frameMargin = new();

        private Sprite logo;
        [CanBeNull] private GameView gameView;
        private readonly GameEnterZone gameEnterZone = new()
        {
            Action = onGameEnter
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = content = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,

                Children =
                [
                    // the game frame container will always have the student pool inside to avoid
                    // having to overwork the positions inside the scroll container n shi
                    flowContainer = new FillFlowContainer()
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Horizontal,
                        Spacing = Vector2.Zero, // initial spacing for good measure
                        Children =
                        [
                            students = new StudentPool()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            frame = new Sprite
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        ]
                    },
                    logo = new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    gameEnterZone,
                ]
            };

            content.ChangeChildDepth(gameEnterZone, -1); // cannot set it manually on the load function of the container
            students.SetTargetContainer(flowContainer);

            gameSession.RequestOwnership(typeof(GameFrameContainer), content, gv => gameView = gv, lostOwnership);

            frameMargin.BindValueChanged((ev) => frame.Margin = ev.NewValue);
        }

        protected override void LoadComplete()
        {
            // instantly cancels the banner transforms
            if (hideBannersOnLoad)
            {
                Hide();
                HideEnterZone();
                ScheduleAfterChildren(() => flowContainer.FinishTransforms(true));
            }

            // schedule the execution of the layout update here for the first time its loaded since pack changed is too eager and the value is not on the bindable yet
            ScheduleAfterChildren(() =>
            {
                gameView?.UpdateLayout(gameUpdate);
                resetZoneBind(true);
            });
        }

        protected override void PackChanged(IResourcePackSource pack)
        {
            gameUpdate = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);

            UpdateSprites();
            UpdateLayout();
            gameView?.UpdateLayout(gameUpdate);
        }

        protected virtual void UpdateSprites()
        {
            Texture frameTex = CurrentPack.GetTexture("Startup/UI/GameFrame.png", WrapMode.None, WrapMode.None, false, true);
            if (frameTex.Size != frame.Texture?.Size) // reset the size if its not the same texture
                frame.Size = Vector2.Zero;
            frameTex.ScaleAdjust = 1.16F;
            frame.Texture = frameTex;

            bool isLivinOnSweets = gameUpdate >= (int)GameUpdateVersion.LivinOnSweets;

            // Atlases should be able to change the filtering at runtime too but for now im gonna leave it like this
            // Now i just realized atlases are just another texture bruh, changing the filtering at runtime would change all the texture filterings in that atlas
            Texture logoTex = CurrentPack.GetTexture("Startup/UI/EventName.png", default, default, false, filteringMode: isLivinOnSweets ? TextureFilteringMode.Nearest : TextureFilteringMode.Linear);
            if (logoTex.Size != logo.Texture?.Size) // reset the size if its not the same texture
                logo.Size = Vector2.Zero;

            logoTex.ScaleAdjust = isLivinOnSweets ? 1.16F : 1.1F;
            logo.Texture = logoTex;
        }

        protected virtual void UpdateLayout()
        {
            // reschedule this update call after children
            if (Parent is null or { Parent: null })
            {
                ScheduleAfterChildren(UpdateLayout);
                return;
            }

            bool isAntiqueSeraphim = gameUpdate >= (int)GameUpdateVersion.AntiqueSeraphim;

            // Updates the top margin to properly position the game container
            MarginPadding parentPad = Parent!.Parent!.Padding; // Parent should def be the scroll container

            MarginPadding margin = Margin;

            // Custom float value (magic) for antique seraphim and greater, this is cuz the container is taller than the base one
            float topPortion = isAntiqueSeraphim ? 3.7F : 2.3F;
            float topMargin = -parentPad.Top / topPortion; // top padding is already negative
            float shadowSpace = frame.Texture.DisplayHeight / 10; // get a bit of the shadow(?) space from the texture - best guess lol

            margin.Top = topMargin + shadowSpace;

            Margin = margin;

            // Updates the margin of the logo
            bool isLivinOnSweets = gameUpdate >= (int)GameUpdateVersion.LivinOnSweets;

            MarginPadding logoMargin = logo.Margin;

            logoMargin.Top = isLivinOnSweets ? 54F : 58F;
            logoMargin.Left = isLivinOnSweets ? 60F : 95F;

            logo.Margin = logoMargin;

            // updates the margin of the frame to adjust the animation
            float pad = isAntiqueSeraphim ? 10 : 18;
            frameMargin.Default = new MarginPadding() { Left = pad * 2 };
            if (!students.IsHidden) // banners are visible, apply the shift instantly
                frameMargin.SetDefault();
        }

        // Silly additions
        public void FadeBannersTo(float alpha, double duration = 0D, Easing easing = Easing.None) => students.FadeBannersTo(alpha, duration, easing);
        public void PushScreen(GameScreenData screenData) => gameView?.PushScreen(screenData);
        public void ResetOwnership()
        {
            gameSession.RequestOwnership(typeof(GameFrameContainer), content, gv =>
            {
                gameView = gv;
                resetGameVisuals();
                resetZoneBind(true);
            }, lostOwnership);
        }

        private void resetGameVisuals()
        {
            if (gameView == null)
                return;

            gameView.GameMargin.SetDefault();
            gameView.RelativeSizeAxes = Axes.None;
            gameView.ContainerSize.SetDefault();
        }

        private void resetZoneBind(bool bind)
        {
            if (bind)
            {
                gameEnterZone.ContainerSize.BindTo(gameView?.ContainerSize);
                gameEnterZone.ContainerMargin.BindTo(gameView?.GameMargin);
            }
            else
            {
                gameEnterZone.ContainerMargin.UnbindBindings();
                gameEnterZone.ContainerSize.UnbindBindings();
            }
        }

        // this should be handled by the startup screen but in order to make it more reusable this is gonna handle it
        public override void Show()
        {
            if (!students.IsHidden)
                return;

            this.TransformBindableTo(frameMargin, frameMargin.Default, 1000, Easing.OutQuint);
            students.Show();
        }

        // the show call should handle showing the enter zone or not, ignoring the state of the students bro
        public void ShowEnterZone() => gameEnterZone.Show();

        public override void Hide()
        {
            if (students.IsHidden)
                return;

            this.TransformBindableTo(frameMargin, new MarginPadding(0), 1000, Easing.OutQuint);
            students.Hide();
        }

        public void HideEnterZone() => gameEnterZone.Hide();

        private void lostOwnership()
        {
            gameView = null;
            resetZoneBind(false);
        }

        public MenuItem[] ContextMenuItems =>
        [
            new("finish session", () => gameView?.Reset()),
            // i would like these to be inside the student pool but thats too much work (checking the moouse input inside the banner bounds)
#if DEBUG
            new("show banners", Show),
            new("hide banners", Hide),
            new($"using {students.CountInUse} banners"),
            new($"available {students.CountAvailable} banner(s)"),
            new($"{students.CountExcessConstructed} banner excess")
#endif
        ];

        protected partial class GameEnterZone : ClickableContainer
        {
            private const double fade_time = 800D;

            [Resolved] private SessionConfig sessionConfig { get; set; }
            [Resolved] private GameStateManager stateManager { get; set; }

            public readonly Bindable<Vector2> ContainerSize = new(Vector2.Zero);
            public readonly BindableMarginPadding ContainerMargin = new();

            private readonly Bindable<bool> touchActive = new();
            private readonly Bindable<GameplayState> gameState = new();

            private Box background;
            private SpriteText indicator;

            private float maxBgAlpha;
            private float minBgAlpha;
            private Colour4 bgColor;

            private string inputState;
            private string gameRuntimeState;
            private bool pulsing;

            [BackgroundDependencyLoader]
            private void load()
            {
                Anchor = Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.None;

                Children =
                [
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White,
                        Alpha = 0,
                    },
                    indicator = new SpriteText()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "",
                        Font = new FontUsage(family: "DNFBitBit", size: 24F),
                        Alpha = 0,
                    }
                ];

                // really overworked but its good to keep up reactivity, will prob be a pain with translations
                sessionConfig.BindWith(SessionSetting.TouchInputActive, touchActive);
                gameState.BindTo(stateManager.GameplayMachine.CurrentState);

                gameState.BindValueChanged((ev) =>
                {
                    if (ev.NewValue > GameplayState.Ready) // not in the main screen
                        return;

                    bool notInit = ev.NewValue < GameplayState.Ready;
                    gameRuntimeState = notInit ? "start the game" : "resume the game";
                    maxBgAlpha = notInit ? 0.15F : 0.55F;
                    minBgAlpha = notInit ? 0F : 0.35F;
                    bgColor = notInit ? Colour4.White : Colour4.Black;

                    // eh looks good, im gonna go with this
                    // the reason why we do this now is because if we reset active loops it can slow down the running loops for some reason
                    // so we just stop and start again
                    if (pulsing)
                    {
                        Hide();
                        Show();
                    }
                }, true);

                touchActive.BindValueChanged((ev) => inputState = (ev.NewValue ? "touch here" : "press enter or click here"), true);

                ContainerSize.BindValueChanged((ev) => Size = ev.NewValue);
                ContainerMargin.BindValueChanged((ev) => Margin = ev.NewValue);
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();
                indicator.Text = $"{inputState} to {gameRuntimeState}";
            }

            public override void Show()
            {
                if (Enabled.Value)
                    return;

                Enabled.Value = true;

                // clear the transforms, then fade in the starting objects then restart the loops
                background.ClearTransforms(true);
                indicator.ClearTransforms(true);

                background.FadeTo(0).Then().FadeTo(maxBgAlpha, fade_time, Easing.OutQuint)
                    .FadeColour(bgColor, fade_time / 4, Easing.OutQuint);

                indicator.FadeTo(1, fade_time, Easing.OutQuint);

                Scheduler.AddDelayed(resetLoops, fade_time + (fade_time / 4));
                pulsing = true;
            }

            public override void Hide()
            {
                if (!Enabled.Value)
                    return;

                // interrupts the previous sequence
                Enabled.Value = false;

                background.ClearTransforms(true);
                indicator.ClearTransforms(true);

                background.FadeTo(0, fade_time, Easing.OutQuint);
                indicator.FadeTo(0, fade_time, Easing.OutQuint);
                pulsing = false;
            }

            private void resetLoops()
            {
                background.ClearTransforms(true);
                indicator.ClearTransforms(true);

                background.Loop(b => b.FadeTo(minBgAlpha, fade_time, Easing.InOutQuart).Then().FadeTo(maxBgAlpha, fade_time, Easing.InOutQuart));
                indicator.Loop(i => i.FadeTo(0.75F, fade_time, Easing.InOutQuart).Then().FadeTo(1F, fade_time, Easing.InOutQuart));
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (Enabled.Value)
                    Action?.Invoke();
                return Enabled.Value;
            }
        }
    }
}
