using LivinOnSweets.API.Input;
using LivinOnSweets.Game.Screens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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
            ActionContainer.Add(screenStack = new ScreenStack()
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            screenStack.Push(new StartupScreen());
        }
    }
}
