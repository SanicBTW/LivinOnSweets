using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.Editor.Containers;

internal partial class EditorScrollContainer : EditorScrollContainer<Drawable>
{
    public const float FREE_HEIGHT = EditorWindow.HEIGHT -
                                        (EditorWindow.INNER_STROKE +
                                        EditorWindowTitleBar.HEIGHT +
                                        EditorWindowOption.HEIGHT);

    public const float BAR_WIDTH = 18;
    public const float BAR_CORNER_RADIUS = 15;

    public EditorScrollContainer(Direction scrollDirection = Direction.Vertical)
        : base(scrollDirection) { }
}

internal partial class EditorScrollContainer<T> : ScrollContainer<T>
    where T : Drawable
{
    public EditorScrollContainer(Direction scrollDirection = Direction.Vertical)
        : base(scrollDirection)
    {
        Position = new Vector2(
            EditorWindow.INNER_STROKE,
            EditorWindowTitleBar.HEIGHT + EditorWindowOption.HEIGHT
        );

        Size = new Vector2(
            EditorWindow.WIDTH - EditorWindow.INNER_STROKE,
            EditorScrollContainer.FREE_HEIGHT
        );
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddContentBackground();
        AddScrollBarBackground();
    }

    // TODO! Replace the rounding cover with a simple rectangle with the real size that it should cover (BAR WIDTH - INNER RAD?)
    protected virtual void AddScrollBarBackground()
    {
        // Since the scroll container moves the scroll bar itself, we have to
        // add another container internally to be able to add the background

        // Remove the scrollbar from the internal children WITHOUT DISPOSING IT
        RemoveInternal(Scrollbar, false);

        // Create the round background (yes, we are making it round although you cant really see it,
        // his is to avoid any visible artifacts at the bottom corner of the window)
        Container bg = new Container()
        {
            Name = "background mask",
            Depth = 2,
            Masking = true,
            CornerRadius = EditorScrollContainer.BAR_CORNER_RADIUS,
            Size = new Vector2(EditorScrollContainer.BAR_WIDTH, EditorScrollContainer.FREE_HEIGHT),
            Child = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = EditorWindow.BackgroundColor
            }
        };

        // Create the radius cover for the top left and bottom left corners
        Box bgRadCover = new Box()
        {
            Name = "background radius cover",
            Depth = 1,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Size = new Vector2(EditorScrollContainer.BAR_WIDTH / 2, EditorScrollContainer.FREE_HEIGHT),
            Colour = EditorWindow.BackgroundColor
        };

        // Anchor!!
        ScrollbarAnchor = Anchor.TopCentre;

        // Now add another container which positions itself correctly inside the scroll container (not the content)
        AddInternal(new Container()
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            // Add the backgrounds, cover and then the scrollbar back again
            Children =
            [
                bg,
                bgRadCover,
                Scrollbar
            ]
        });
    }

    protected virtual void AddContentBackground()
    {
        // Remove the scroll content WITHOUT DISPOSING IT
        RemoveInternal(ScrollContent, false);

        // Make the background the same as the window background (since the next background is gonna be masked)
        Box background = new Box()
        {
            Name = "scroll background",
            Depth = 3,
            RelativeSizeAxes = Axes.Both,
            Colour = EditorWindow.BackgroundColor
        };

        // Make the inner rounded background, sits on top of the "window" background
        Container innerBackground = new Container()
        {
            Name = "scroll inner background",
            Masking = true,
            Depth = 2,
            CornerRadius = EditorWindow.INNER_STROKE,
            RelativeSizeAxes = Axes.Both,
            Child = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = EditorWindow.InnerBackgroundColor
            }
        };

        // Make the inner background radius cover, which covers the top left and top right corners
        Box innerBgRadCover = new Box()
        {
            Name = "scroll inner background radius cover",
            Depth = 1,
            RelativeSizeAxes = Axes.X,
            Height = EditorWindow.INNER_STROKE / 2,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Colour = EditorWindow.InnerBackgroundColor
        };

        // I'm not masking the scroll container since I cannot hide any of its corners easily, so fuck it for now, not like its gonna bother someone, right?

        AddInternal(new Container()
        {
            Width = EditorWindow.WIDTH - (EditorWindow.INNER_STROKE + EditorScrollContainer.BAR_WIDTH),
            RelativeSizeAxes = Axes.Y,
            Children =
            [
                background,
                innerBackground,
                innerBgRadCover,
                ScrollContent
            ]
        });
    }

    protected override ScrollbarContainer CreateScrollbar(Direction direction) => new EditorScrollBar(direction);

    protected partial class EditorScrollBar : ScrollbarContainer
    {
        private const float dim_size = 8;

        private Colour4 barColor = Colour4.FromHex("#464D78");

        public EditorScrollBar(Direction direction)
            : base(direction)
        {
            Child = new Container()
            {
                Name = "handle mask",
                Masking = true,
                CornerRadius = 3,
                RelativeSizeAxes = Axes.Both,
                Child = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = barColor
                }
            };
        }

        // From SweetScrollBar
        public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
        {
            Vector2 size = new Vector2(dim_size)
            {
                [(int)ScrollDirection] = val
            };
            this.ResizeTo(size, duration, easing);
        }
    }
}
