using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites.UI
{
    // Literally https://github.com/ppy/osu/blob/master/osu.Game/Graphics/UserInterface/LoadingSpinner.cs
    // TODO: Make a custom design rather than just copying osu lazer
    public partial class LoadingSpinner : VisibilityContainer
    {
        public const double TRANSITION_DURATION = 500;
        private const double spin_duration = 900;

        protected override bool StartHidden => true;

        protected OContainer MainContents;
        private readonly SpriteIcon spinner;

        public LoadingSpinner(bool withBox = false, bool inverted = false)
        {
            Size = new Vector2(60);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Child = MainContents = new OContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 20,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new Box
                    {
                        Colour = inverted ? Colour4.White : Colour4.Black,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = withBox ? 0.7f : 0
                    },
                    spinner = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = inverted ? Colour4.Black : Colour4.White,
                        Scale = new Vector2(withBox ? 0.6f : 1),
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.Solid.CircleNotch
                    }
                ]
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Rotate();
        }

        protected override void Update()
        {
            base.Update();

            MainContents.CornerRadius = MainContents.DrawWidth / 4;
        }

        protected override void PopIn()
        {
            if (Alpha < 0.5f)
                Rotate();

            MainContents.ScaleTo(1, TRANSITION_DURATION, Easing.OutQuint);
            this.FadeIn(TRANSITION_DURATION * 2, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            MainContents.ScaleTo(0.8f, TRANSITION_DURATION / 2, Easing.In);
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected virtual void Rotate()
        {
            spinner.Spin(spin_duration * 3.5f, RotationDirection.Clockwise);

            MainContents.RotateTo(0).Then()
                        .RotateTo(90, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(180, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(270, spin_duration, Easing.InOutQuart).Then()
                        .RotateTo(360, spin_duration, Easing.InOutQuart).Then()
                        .Loop();
        }
    }
}
