using LivinOnSweets.API.Components;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Sprites.Editor
{
    internal partial class EditorSlider : SlideContainer, IAccentColorReceiver
    {
        [Resolved]
        private AccentComponent accentComponent { get; set; }

        protected BindableColour4 PrimaryColor = new();
        protected BindableColour4 SecondaryColor = new();

        internal Box ContainerBackground = new()
        {
            RelativeSizeAxes = Axes.Both
        };

        internal SweetScrollContainer ScrollContent = new()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Both
        };

        protected SlideContainer ParentSlider;
        protected EditorSliderHeader Header;

        internal bool Closing = false;

        public EditorSlider(bool leftSide, SlideContainer parentSlider) : base(leftSide)
        {
            ParentSlider = parentSlider;

            Children =
            [
                ContainerBackground,
                Header = new EditorSliderHeader(this),
                ScrollContent
            ];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PrimaryColor.BindValueChanged((ev) =>
            {
                ContainerBackground.Colour = ev.NewValue;
            });

            Width = ParentSlider.Width * 1.5f;
            RelativeSizeAxes = ParentSlider.RelativeSizeAxes;

            accentComponent.PropagateToChildren(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PanelNudge.NudgeColor.BindTo(ParentSlider.PanelNudge.NudgeColor);
            SlideBlock.BindTo(ParentSlider.SlideBlock);
            ClickOutClosesContainer = ParentSlider.ClickOutClosesContainer;

            CalculateScrollerPosition();

            this.MoveToX(0, SlideDuration, Easing.OutQuint);
        }

        protected virtual void CalculateScrollerPosition()
        {
            float headerHeight = Header.Height;
            float targetScrollerHeight = ScrollContent.DrawHeight - headerHeight;
            float newRelativeSize = 1 / (ScrollContent.DrawHeight / targetScrollerHeight);

            ScrollContent.Height = newRelativeSize;
            ScrollContent.Y = headerHeight;
        }

        // Overriden base hover methods to add a quick flag to avoid triggering animations which would cause on the
        // container transitioning to X = 0 again and thus cancelling the previous transform, in this case being the close animation
        protected override bool OnHover(HoverEvent e)
        {
            if (Closing)
                return true;

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (Closing)
                return;

            base.OnHoverLost(e);
        }

        AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

        void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
        {
            PrimaryColor.BindTo(colors[1]);
            SecondaryColor.BindTo(colors[2]);
        }

        void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
        {
            BindableColour4 newPrimary = accentComponent.GetAccent(this, AccentColorRole.Secondary);
            BindableColour4 newSecondary = accentComponent.GetAccent(this, AccentColorRole.Tertiary);

            this.TransformBindableTo(PrimaryColor, newPrimary.Value, duration, easing);
            this.TransformBindableTo(SecondaryColor, newSecondary.Value, duration, easing);
        }
    }
}
