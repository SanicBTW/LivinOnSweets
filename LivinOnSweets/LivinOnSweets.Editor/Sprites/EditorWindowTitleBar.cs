using LivinOnSweets.Editor.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/TitleBar.cs
namespace LivinOnSweets.Editor.Sprites;

internal partial class EditorWindowTitleBar : CompositeDrawable
{
    public const float HEIGHT = 60;
    public const float CORNER_RADIUS = 15;

    private readonly Drawable movableTarget;

    public EditorWindowTitleBar(TranslatableString title, Drawable movableTarget)
    {
        this.movableTarget = movableTarget;

        Depth = 2;
        Size = new Vector2(EditorWindow.WIDTH, HEIGHT);

        InternalChildren =
        [
            new Container()
            {
                Name = "background masking",
                Depth = 2,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = CORNER_RADIUS,
                Child = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = EditorWindow.BackgroundColor
                }
            },
            // Simulate the custom corner radius from the figma
            new Box()
            {
                Name = "background radius cover",
                Depth = 1,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = CORNER_RADIUS,
                Colour = EditorWindow.BackgroundColor
            },
            new SpriteText()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = title,
                Font = new FontUsage(family: "GyeonggiTitle", size: 24F)
            }
        ];
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDrag(DragEvent e)
    {
        movableTarget.Position += e.Delta;
        base.OnDrag(e);
    }

    protected override bool OnMouseDown(MouseDownEvent e) => true;
}
