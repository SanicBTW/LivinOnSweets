using LivinOnSweets.API.Components;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.API.Sprites.Editor
{
    // TODO: Fix design having issues with mouse events (they get fired even if its out of bounds, probably due to the padding)
    // TODO (Children): Bind the scrollbar colour to the parent of the controller which is indeed, a scroll container
    public partial class EditorEntry : Container, IAccentColorReceiver
    {
        [Resolved]
        private AccentComponent accentComponent { get; set; }

        protected BindableColour4 PrimaryColor = new();
        protected BindableColour4 SecondaryColor = new();

        internal EntryHeader Entryheader;

        // The parent container of all the content inside this entry
        protected Container RoundedMask;
        private Box background;

        public readonly string Category;
        public readonly EditorEntryContentAnimation ContentAnimation;
        public double ColorFadeDuration = 500D;
        public float BaseHeight = 150;

        public EditorEntry(string category, EditorEntryContentAnimation entryType)
        {
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

            PrimaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(PrimaryColor.Value.Darken(0.15f), ColorFadeDuration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(PrimaryColor.Value, ColorFadeDuration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected virtual EntryHeader CreateHeader(string category) => new(category);

        protected virtual EntryPreview CreatePreview() => new EntryPreviewPlaceholder();

        protected virtual EntryExpandable CreateExpandable() => new EntryExpandablePlaceholder(this);

        AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

        void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
        {
            // This mimics the old behaviour of EditorSideBar, the secondary was the primary and the tertiary was the secondary
            // kind of asss if you ask me, I never realized it until now lmao
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
