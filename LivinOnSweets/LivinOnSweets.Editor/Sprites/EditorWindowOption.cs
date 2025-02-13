using JetBrains.Annotations;
using LivinOnSweets.Editor.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.Editor.Sprites;

internal partial class EditorWindowOption : CompositeDrawable
{
    public const float HEIGHT = 32;
    public const float CORNER_RADIUS = 15;

    public static MarginPadding TitleMargin = new()
    {
        Left = EditorWindow.INNER_STROKE,
        Right = 18,
    };

    public static Colour4 ForegroundColor = Colour4.FromHex("#1B1E2D");

    private readonly Action action;

    public EditorWindowOption(string label, [CanBeNull] Action action = null)
    {
        this.action = action;

        Depth = 1;
        Size = new Vector2(EditorWindow.WIDTH - TitleMargin.TotalHorizontal, HEIGHT);

        // TODO: Remove this behaviour when adding to a FillFlowContainer
        // Leave a little bit of margin to the left, this should be done with the margin itself but uhh yeah
        X = TitleMargin.Left;
        // Start at the height of the title bar, since we are not in a fill flow container, we have to set the position manually
        Y = EditorWindowTitleBar.HEIGHT;

        InternalChildren =
        [
            new Box()
            {
                Name = "background",
                Depth = 3,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // since the container isn't masked, we can go outside the bounds of the option, kind of a bad option, but a quick hack
                Width = EditorWindow.WIDTH,
                RelativeSizeAxes = Axes.Y,
                Colour = EditorWindow.BackgroundColor
            },
            new Container()
            {
                Name = "foreground masking",
                Depth = 2,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = CORNER_RADIUS,
                Child = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ForegroundColor
                }
            },
            // Same trick as TitleBar
            new Box()
            {
                Name = "foreground radius cover",
                Depth = 1,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = CORNER_RADIUS,
                Colour = ForegroundColor,
            },
            new SpriteText()
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = label,
                Font = new FontUsage(family: "GyeonggiTitle", size: 12F),
                Margin = new MarginPadding() { Left = CORNER_RADIUS }
            }
        ];
    }

    protected override bool OnClick(ClickEvent e)
    {
        action?.Invoke();
        return true;
    }
}
