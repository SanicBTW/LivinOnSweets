using LivinOnSweets.API.Components;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enum;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Sprites.Editor
{
    // TODO: Fix when dragging the scroll container the expandable will act like it was a click and thus toggle the bindable
    public partial class EntryExpandable : Container, IAccentColorReceiver
    {
        public const float BASE_HEIGHT = 92;
        public const int EXPAND_MULT = 2;

        [Resolved]
        private AccentComponent accentComponent { get; set; }

        protected BindableColour4 PrimaryColor = new();
        protected BindableColour4 SecondaryColor = new();

        protected override Container Content => ExpandedContent;
        protected readonly Container ExpandedContent;

        protected EditorEntry Entry;
        protected Box Background;

        private BindableBool locked = new();
        public double ResizeDuration = 750D;

        public EntryExpandable(EditorEntry entry)
        {
            Entry = entry;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            Height = BASE_HEIGHT;
            Padding = new MarginPadding(8);
            Y = entry.Entryheader.Height - entry.Entryheader.Padding.Bottom; // dont take the bottom padding into account since the top of THIS padding will act like it between these 2

            InternalChild = new PositionallyLockedContainer(locked)
            {
                Masking = true,
                CornerRadius = 6f,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Background = new Box()
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
            // sanco here, i dont really know if this even works as i intended i should totally check this again
            // TODO: Check this behaviour
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

        AccentBannerSide IAccentColorReceiver.AccentSide => AccentBannerSide.Left;

        void IAccentColorReceiver.PropagateAccents(BindableColour4[] colors)
        {
            PrimaryColor.BindTo(colors[1]);
            SecondaryColor.BindTo(colors[2]);

            SecondaryColor.BindValueChanged((ev) =>
            {
                Background.Colour = ev.NewValue;
            });
        }

        void IAccentColorReceiver.AccentsUpdated(double duration, Easing easing)
        {
            BindableColour4 newPrimary = accentComponent.GetAccent(this, AccentColorRole.Secondary);
            BindableColour4 newSecondary = accentComponent.GetAccent(this, AccentColorRole.Tertiary);

            this.TransformBindableTo(PrimaryColor, newPrimary.Value, duration, easing);
            this.TransformBindableTo(SecondaryColor, newSecondary.Value, duration, easing);
        }
    }

    // Basic container that overrides the propagation of the positional input through a bindable bool that acts like a lock for it
    // Useful if the content inside the container should not be interacted with through mouse events until unlocked
    internal partial class PositionallyLockedContainer(BindableBool lockBindable) : Container
    {
        public override bool PropagatePositionalInputSubTree => lockBindable.Value;
    }

    internal partial class EntryExpandablePlaceholder(EditorEntry entry) : EntryExpandable(entry)
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

            PrimaryColor.BindValueChanged((ev) =>
            {
                text.Colour = ev.NewValue;
            }, true);
        }
    }
}
