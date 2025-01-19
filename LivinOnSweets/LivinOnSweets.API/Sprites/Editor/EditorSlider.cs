using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Sprites.Editor
{
    internal partial class EditorSlider : SlideContainer
    {
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
        protected EditorSideBar Controller;

        internal bool Closing = false;

        public EditorSlider(bool leftSide, SlideContainer parentSlider, EditorSideBar controller) : base(leftSide)
        {
            ParentSlider = parentSlider;
            Controller = controller;

            Children =
            [
                ContainerBackground,
                Header = new EditorSliderHeader(this, controller),
                ScrollContent
            ];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Controller.PrimaryColor.BindValueChanged((ev) =>
            {
                ContainerBackground.Colour = ev.NewValue;
            }, true);

            Width = ParentSlider.Width * 1.5f;
            RelativeSizeAxes = ParentSlider.RelativeSizeAxes;
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
    }
}
