using LivinOnSweets.Editor.Entries;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.Editor.Containers;

internal partial class ToolBar : AutoSizeOnceContainer
{
    public ToolBar() : base(Axes.Y)
    {
        AutoSizeAxes = Axes.Both;
        Anchor = Anchor.BottomCentre;
        Origin = Anchor.BottomCentre;
        Masking = true;
        CornerRadius = 15;
        Margin = new MarginPadding() { Bottom = 26 };
        Depth = -99;

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
                    new ResourcePackExplorer(),
                    new SongTestWindow(),
                    new GameSessionExplorer(),
                    new ModLoaderExplorer(),
                    new RestartCurrentScreen(),
                ]
            }
        ]);
    }
}
