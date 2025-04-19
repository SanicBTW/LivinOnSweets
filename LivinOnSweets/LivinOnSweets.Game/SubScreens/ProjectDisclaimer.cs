using LivinOnSweets.API.Graphics.UserInterface;
using LivinOnSweets.API.Screens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.SubScreens
{
    public partial class ProjectDisclaimer : SweetSubScreen
    {
        private const double transition_duration = 500;

        private Container mainContent;
        private TypeWriterText typeWriter;
        private CircularProgress timeLeft;
        private bool exiting;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = DrawSize / 2;
            Alpha = 0;

            InternalChild = mainContent = new Container
            {
                Scale = new Vector2(0.8F),
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new Box
                    {
                        Colour = Colour4.DarkGray.Darken(8F),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Exclamation,
                        Size = new Vector2(48F),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding(8)
                    },
                    typeWriter = new TypeWriterText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    timeLeft = new CircularProgress
                    {
                        Size = new Vector2(32),
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding(8),
                        Alpha = 0,
                        InnerRadius = 0.2F
                    }
                ]
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RelativeSizeAxes = Axes.None; // Disable relative sizes since scaling wont take any affect (we dont want that!!)
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            // Not really visible but aight
            mainContent.ScaleTo(1, transition_duration, Easing.OutQuint);
            this.FadeIn(transition_duration * 2, Easing.OutQuint);

            Scheduler.AddDelayed(() =>
            {
                // Translatable string gang
                typeWriter.Start("This game is only a fan project\n" +
                                            "It's still heavily on work in progress\n" +
                                            "Please report any issues you might find\n\n" +
                                            "All rights reserved to\n"+
                                            "NEXON Korea Corp. & NEXON Games Co., LTD", 25);
            }, 500);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            mainContent.ScaleTo(0.8F, transition_duration / 2, Easing.In);
            this.FadeOut(transition_duration, Easing.OutQuint);
            return base.OnExiting(e);
        }

        protected override void Update()
        {
            base.Update();

            // Once its done writing, wait a little bit, then close the subscreen, changing to the next screen
            if (!typeWriter.IsFinished || exiting) return;

            exiting = true;
            Scheduler.AddDelayed(Exit, 5000D);
            timeLeft.FadeIn(500D, Easing.OutQuint).ProgressTo(1D, 5000D, Easing.OutQuart);
        }
    }
}
