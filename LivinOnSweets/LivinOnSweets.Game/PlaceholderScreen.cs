using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;

namespace LivinOnSweets.Game
{

    public partial class PlaceholderScreen : Screen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.White
            };
        }
    }
}
