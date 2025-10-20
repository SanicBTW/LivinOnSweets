using LivinOnSweets.Editor.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.Editor.Sprites;

internal partial class Card : Container
{
    public const float CARD_MARGIN = 6;
    public const float CARD_WIDTH = EditorWindow.WIDTH - ((EditorWindow.INNER_STROKE + CARD_MARGIN) * 2);
    public const float CARD_HEIGHT = 82;

    protected override Container<Drawable> Content => container;

    private readonly Container<Drawable> container = new()
    {
        RelativeSizeAxes = Axes.Both,
    };

    protected Card()
    {
        Masking = true;
        CornerRadius = 10;
        // god i love magic numbers
        Size = new Vector2(CARD_WIDTH - (CARD_MARGIN * 1.75F), CARD_HEIGHT);

        // should make the uhh last card of the list to automatically set the bottom margin
        Margin = new MarginPadding() { Left = CARD_MARGIN, Top = CARD_MARGIN };

        InternalChildren =
        [
            new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = EditorWindow.BackgroundColor
            },
            container
        ];
    }
}
