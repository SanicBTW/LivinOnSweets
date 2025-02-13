using LivinOnSweets.Editor.Enum;
using LivinOnSweets.Editor.Sprites;
using LivinOnSweets.Editor.ToolBarEntries;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.Editor.Containers;

internal partial class ToolBar : Container
{
    public ToolBar()
    {
        AutoSizeAxes = Axes.Both;
        Anchor = Anchor.BottomCentre;
        Origin = Anchor.BottomCentre;
        Masking = true;
        CornerRadius = 15;
        Margin = new MarginPadding() { Bottom = 26 };
        Depth = -99; // Topmost

        AddRangeInternal([
            new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.FromHex("#23273E")
            },
            new FillFlowContainer<ToolBarButton>()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(24, 0),
                Padding = new MarginPadding() { Top = 14, Bottom = 14, Left = 22, Right = 22 },
                Children =
                [
                    new StackInspector(),
                    new ToolBarButton(FontAwesome.Solid.EyeDropper, ToolBarActionType.STANDARD),
                ]
            }
        ]);
    }

    protected override void UpdateAfterAutoSize()
    {
        base.UpdateAfterAutoSize();

        // This disables the auto sizing on the Y axis while also keeping the height before disabling the auto sizing
        // this is made to be able to modify the sprites without worrying about the size of this container changing
        if (AutoSizeAxes.HasFlagFast(Axes.Y))
        {
            float prevHeight = DrawHeight;
            AutoSizeAxes &= ~Axes.Y;
            Height = prevHeight;
        }
    }
}
