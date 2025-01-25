using LivinOnSweets.API.Containers.Editor;
using LivinOnSweets.API.Enum;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.API.Sprites.Editor
{
    // TODO: Fix design having issues with mouse events (they get fired even if its out of bounds, probably due to the padding)
    // TODO (Children): Bind the scrollbar colour to the parent of the controller which is indeed, a scroll container
    // Controller as the variable name for EditorSideBar is kinda misleading ngl
    public partial class EditorEntry : Container
    {
        public readonly string Category;
        public readonly EditorEntryContentAnimation ContentAnimation;
        public double ColorFadeDuration = 500D;
        public float BaseHeight = 150;

        protected Container RoundedMask; // The parent container of all the content inside this entry
        protected EditorSideBar Controller;
        protected ScreenStack TargetScreenStack;

        internal EntryHeader Entryheader;

        private Box background;

        public EditorEntry(EditorSideBar controller, string category, EditorEntryContentAnimation entryType)
        {
            Controller = controller;
            Category = category;
            ContentAnimation = entryType;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            // Figma reported this size so I'm doing it :grin:
            Size = new Vector2(400, BaseHeight);
            Padding = new MarginPadding(6);

            InternalChild = RoundedMask = new Container()
            {
                Masking = true,
                CornerRadius = 6f,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RoundedMask.Add(Entryheader = CreateHeader(Category));
            switch (ContentAnimation)
            {
                case EditorEntryContentAnimation.EXPANDABLE:
                    RoundedMask.Add(CreateExpandable());
                    break;

                case EditorEntryContentAnimation.ANOTHER_VIEW:
                    RoundedMask.Add(CreatePreview());
                    break;
            }

            Controller.PrimaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(Controller.PrimaryColor.Value.Darken(0.15f), ColorFadeDuration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(Controller.PrimaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected virtual EntryHeader CreateHeader(string category) => new(category, Controller);

        protected virtual EntryPreview CreatePreview() => new EntryPreviewPlaceholder(Controller);

        protected virtual EntryExpandable CreateExpandable() => new EntryExpandablePlaceholder(this, Controller);

        internal void ChangeScreenCtx(ScreenStack newCtx) => TargetScreenStack = newCtx;
    }
}
