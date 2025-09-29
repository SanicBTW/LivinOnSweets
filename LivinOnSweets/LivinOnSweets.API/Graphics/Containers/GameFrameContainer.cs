using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Graphics.Sprites.Startup;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

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
        private Sprite frame;
        // this is such a horrible hack omg
        // since the frame container now has a fill flow container for the banners n shit
        // now when showing the banners the frame shifts a little bit, so we use a margin which doubles that amount
        // to reshift the container while doing the sliding on the banners, not my favorite trick but looks good
        private readonly BindableMarginPadding frameMargin = new();
        private Sprite logo;
        private Bindable<GameView> gameView = new();
        private StudentPool students;

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
                        Spacing = new Vector2(0, 0), // initial spacing for good measure
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
                ]
            };

            students.SetTargetContainer(flowContainer);
            gameSession.RequestOwnership(typeof(GameFrameContainer), content, gameView);

            frameMargin.BindValueChanged((ev) => frame.Margin = ev.NewValue);
        }

        protected override void LoadComplete()
        {
            // instantly cancels the banner transforms
            if (hideBannersOnLoad)
            {
                Hide();
                ScheduleAfterChildren(() => flowContainer.FinishTransforms(true));
            }

            // execute the update layout here for the first time its loaded since pack changed is too eager and the value is not on the bindable yet
            gameView.Value?.UpdateLayout(gameUpdate);
        }

        protected override void PackChanged(IResourcePackSource pack)
        {
            gameUpdate = (int)sweetConfig.Get<GameUpdateVersion>(SweetSetting.GameUpdate);

            UpdateSprites();
            UpdateLayout();
            gameView.Value?.UpdateLayout(gameUpdate);
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
            Texture logoTex = CurrentPack.GetTexture("Startup/UI/EventName.png");
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

        protected override bool OnClick(ClickEvent e)
        {
            // actually this should only happen when clicking the game itself, not the entire container.
            if (e.Button == MouseButton.Left)
            {
                onGameEnter();
                return true;
            }

            return false;
        }

        // Silly additions
        public void FadeBannersTo(float alpha, double duration = 0D, Easing easing = Easing.None) => students.FadeBannersTo(alpha, duration, easing);

        public void PushScreen(GameScreenData screenData) => gameView.Value?.PushScreen(screenData);
        public void ResetOwnership()
        {
            gameView.ValueChanged += resetGameVisuals;
            gameSession.RequestOwnership(typeof(GameFrameContainer), content, gameView);

            Scheduler.AddOnce(() =>
            {
                // since the request takes place inside the scheduler (because it modifies the tree and has to be on update)
                // we check in the scheduler too if the ownership was recovered, if not we throw
                if (gameView.Value == null)
                    throw new InvalidOperationException($"Failed to recover ownership over {nameof(gameView)}");
            });
        }

        private void resetGameVisuals(ValueChangedEvent<GameView> ev)
        {
            GameView game = ev.NewValue;
            game.GameMargin.SetDefault();
            game.RelativeSizeAxes = Axes.None;
            game.ContainerSize.SetDefault();
            gameView.ValueChanged -= resetGameVisuals;
        }

        // this should be handled by the startup screen but in order to make it more reusable this is gonna handle it
        public override void Show()
        {
            if (!students.IsHidden)
                return;

            this.TransformBindableTo(frameMargin, frameMargin.Default, 1000, Easing.OutQuint);
            students.Show();
        }

        public override void Hide()
        {
            if (students.IsHidden)
                return;

            this.TransformBindableTo(frameMargin, new MarginPadding(0), 1000, Easing.OutQuint);
            students.Hide();
        }

        public MenuItem[] ContextMenuItems =>
        [
            new("reset session", () =>
            {

            }),
            // i would like these to be inside the student pool but thats too much work (checking the moouse input inside the banner bounds)
#if DEBUG
            new("show banners", Show),
            new("hide banners", Hide),
            new($"using {students.CountInUse} banners"),
            new($"available {students.CountAvailable} banner(s)"),
            new($"{students.CountExcessConstructed} banner excess")
#endif
        ];
    }
}
