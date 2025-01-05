using LivinOnSweets.API.Input;
using LivinOnSweets.Game.StartScreens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;

namespace LivinOnSweets.Game
{
    public partial class LivinOnSweetsGame : LivinOnSweetsGameBase
    {
        // We are 100% sure that the Parent (LivinOnSweetsGameBase) first and only child is gonna be the input container
        protected ManiaActionContainer ActionContainer => (ManiaActionContainer)Content.Child;

        private ScreenStack screenStack;

        [BackgroundDependencyLoader]
        private void load()
        {
            ActionContainer.AddRange(new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#3a3a3a") // color of the game container
                },
                screenStack = new ScreenStack()
                {
                    RelativeSizeAxes = Axes.Both
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            screenStack.Push(new StartupScreen());
        }
    }
}
