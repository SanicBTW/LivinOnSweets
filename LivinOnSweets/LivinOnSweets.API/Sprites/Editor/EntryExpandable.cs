using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Containers.Editor;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Sprites.Editor
{
    public partial class EntryExpandable : Container
    {
        public const float BASE_HEIGHT = 92;
        public const int EXPAND_MULT = 2;

        protected override Container Content => ExpandedContent;
        protected readonly Container ExpandedContent;

        protected EditorEntry Entry;
        protected EditorSideBar Controller;

        private BindableBool locked = new();

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
            InternalChild = new PositionallyLockedContainer(locked)
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
                        Child = ExpandedContent = new Container()
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // bind the function here so the virtual function is fully applied if overriden
            locked.BindValueChanged(HandleLockChange);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // if blocked, do not set the locked toggle
            bool ret = base.OnMouseDown(e);

            if (!ret)
                locked.Toggle();

            return ret;
        }

        public bool IsCollapsed() => Height <= BASE_HEIGHT;

        public bool IsExpanded() => Height > BASE_HEIGHT;

        protected virtual void HandleLockChange(ValueChangedEvent<bool> ev)
        {
            if (ev.NewValue)
            {
                Entry.ResizeHeightTo(Entry.BaseHeight * EXPAND_MULT, ResizeDuration, Easing.OutQuart);
                this.ResizeHeightTo(BASE_HEIGHT * (EXPAND_MULT + 0.65f), ResizeDuration, Easing.OutQuart);
            }
            else
            {
                this.ResizeHeightTo(BASE_HEIGHT, ResizeDuration, Easing.OutQuart);
                Entry.ResizeHeightTo(Entry.BaseHeight, ResizeDuration, Easing.OutQuart);
            }
        }
    }

    // Basic container that overrides the propagation of the positional input through a bindable bool that acts like a lock for it
    // Useful if the content inside the container should not be interacted with through mouse events until unlocked
    internal partial class PositionallyLockedContainer(BindableBool lockBindable) : Container
    {
        public override bool PropagatePositionalInputSubTree => lockBindable.Value;
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
