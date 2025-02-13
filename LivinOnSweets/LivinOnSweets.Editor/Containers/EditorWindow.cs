using LivinOnSweets.Editor.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/ToolWindow.cs
namespace LivinOnSweets.Editor.Containers;

internal partial class EditorWindow : OverlayContainer
{
    public const float WIDTH = 330;
    public const float HEIGHT = 442;
    public const float INNER_STROKE = 8;

    public static Colour4 BackgroundColor = Colour4.FromHex("#23273E");
    public static Colour4 InnerBackgroundColor = Colour4.FromHex("#2E334A");

    public FillFlowContainer ScrollContent { get; protected set; }

    public EditorWindow(string title, EditorWindowOption option)
    {
        Size = new Vector2(WIDTH, HEIGHT);
        Masking = true;
        CornerRadius = 15;

        AddRangeInternal([
            getBackground(),
            new EditorWindowTitleBar(title, this),
            option,
            new EditorScrollContainer()
            {
                Child = ScrollContent = new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            }
        ]);
    }

    private Container getBackground()
    {
        Container bgContainer = new Container()
        {
            Name = "background container",
            RelativeSizeAxes = Axes.Both,
            Depth = 3,
            Children =
            [
                new Box()
                {
                    Name = "outer background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColor,
                    Depth = 1,
                },
                // Simulate the inner background as the inner stroke from the figma
                new Container()
                {
                    Name = "inner padding",
                    Depth = 0,
                    Padding = new MarginPadding(INNER_STROKE),
                    RelativeSizeAxes = Axes.Both,
                    Child = new Container()
                    {
                        Name = "inner background",
                        Masking = true,
                        CornerRadius = INNER_STROKE,
                        RelativeSizeAxes = Axes.Both,
                        Child = new Box()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = InnerBackgroundColor,
                        }
                    }
                },
            ]
        };

        return bgContainer;
    }

    protected override void PopIn() => this.FadeIn(100);

    protected override void PopOut() => this.FadeOut(100);
}
