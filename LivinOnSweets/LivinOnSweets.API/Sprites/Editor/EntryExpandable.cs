using LivinOnSweets.API.Container;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using OContainer = osu.Framework.Graphics.Containers.Container;

namespace LivinOnSweets.API.Sprites.Editor
{
    public partial class EntryExpandable : OContainer
    {
        public const float BASE_HEIGHT = 92;
        public const int EXPAND_MULT = 2;

        protected override Container<Drawable> Content => ExpandedContent;
        protected readonly Container<Drawable> ExpandedContent;

        protected EditorEntry Entry;
        protected EditorSideBar Controller;

        private bool locked;

        public double ResizeDuration = 750D;

        public EntryExpandable(EditorEntry entry, EditorSideBar controller)
        {
            Entry = entry;
            Controller = controller;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            Height = BASE_HEIGHT;
            Padding = new MarginPadding(8);
            Y = entry.Entryheader.Height - entry.Entryheader.Padding.Bottom; // dont take the bottom padding into account since the top of THIS padding will act like it between these 2

            Box background;
            InternalChild = new OContainer()
            {
                Masking = true,
                CornerRadius = 6f,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SweetScrollContainer()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Child = ExpandedContent = new Container<Drawable>()
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            };

            controller.SecondaryColor.BindValueChanged((ev) =>
            {
                background.Colour = ev.NewValue;
            }, true);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // if blocked, do not set the locked toggle
            bool ret = base.OnMouseDown(e);

            if (!ret)
                locked = !locked;

            return ret;
        }

        protected override bool OnHover(HoverEvent e)
        {
            Entry.ResizeHeightTo(Entry.BaseHeight * EXPAND_MULT, ResizeDuration, Easing.OutQuart);
            this.ResizeHeightTo(BASE_HEIGHT * (EXPAND_MULT + 0.65f), ResizeDuration, Easing.OutQuart);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (locked)
                return;

            this.ResizeHeightTo(BASE_HEIGHT, ResizeDuration, Easing.OutQuart);
            Entry.ResizeHeightTo(Entry.BaseHeight, ResizeDuration, Easing.OutQuart);
            base.OnHoverLost(e);
        }

        public bool IsCollapsed() => Height <= BASE_HEIGHT;

        public bool IsExpanded() => Height > BASE_HEIGHT;
    }

    internal partial class EntryExpandablePlaceholder(EditorEntry entry, EditorSideBar controller) : EntryExpandable(entry, controller)
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            SpriteText text;
            ExpandedContent.Add(text = new SpriteText()
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = new FontUsage(family: "DNFBitBit", size: 32F),
                Text = "placeholder",
                Margin = new MarginPadding() { Left = Padding.Left / 2 },
                Colour = Colour4.Black
            });

            Controller.PrimaryColor.BindValueChanged((ev) =>
            {
                text.Colour = ev.NewValue;
            }, true);
        }
    }
}
