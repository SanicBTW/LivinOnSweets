using LivinOnSweets.API.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace LivinOnSweets.Game.SubScreens
{
    public partial class ProjectDisclaimer : DrawSizePreservingFillContainer
    {
        private const double transition_duration = 500;

        private Container mainContent;
        private TypeWriterText typeWriter;
        private CircularProgress timeLeft;
        private bool exiting;

        public ProjectDisclaimer()
        {
            // Followed my design instinct (figma) and used its sizes
            TargetDrawSize = new Vector2(826, 592);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;

            InternalChild = mainContent = new DrawSizePreservingFillContainer
            {
                TargetDrawSize = TargetDrawSize,
                Scale = new Vector2(0.8F),
                Masking = true,
                CornerRadius = 5,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new Container
                {
                    Size = TargetDrawSize,
                    Position = new Vector2(10, -6),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.71F),
                    Children =
                    [
                        new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Exclamation,
                            Size = new Vector2(48F),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Top = 16, Left = 4 }
                        },
                        typeWriter = new TypeWriterText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Top = 12, Left = 56 }, // (icon) Margin.Left + Width + sum lil extra margin to look good
                            Padding = new MarginPadding { Right = 58 }, // the same left margin but with sum more extra to look wrap text correctly
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.Both,
                        },
                        timeLeft = new CircularProgress
                        {
                            Size = new Vector2(48),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Margin = new MarginPadding(12),
                            Alpha = 0,
                            InnerRadius = 0.2F
                        }
                    ]
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (BeginDelayedSequence(500))
            {
                // Not really visible but aight
                mainContent.ScaleTo(1, transition_duration, Easing.OutQuint);
                this.FadeIn(transition_duration * 2, Easing.OutQuint);

                Scheduler.AddDelayed(() => typeWriter.Start(new TranslatableString("startup:disclaimer", "placeholder placeholder"), 20), 500);
            }
        }

        protected override void Update()
        {
            base.Update();

            // Once its done writing, wait a little bit, then close the subscreen, changing to the next screen
            if (!typeWriter.IsFinished || exiting) return;

            exiting = true;
            timeLeft.FadeIn(500D, Easing.OutQuint).ProgressTo(1D, 5000D, Easing.OutQuint).OnComplete(_ => exit());
        }

        private void exit()
        {
            mainContent.ScaleTo(0.8F, transition_duration / 2, Easing.In);
            this.FadeOut(transition_duration, Easing.OutQuint);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            this.TransformBindableTo(typeWriter.Speed, 10, 250D);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            this.TransformBindableTo(typeWriter.Speed, typeWriter.Speed.Default, 250D);
        }
    }
}
