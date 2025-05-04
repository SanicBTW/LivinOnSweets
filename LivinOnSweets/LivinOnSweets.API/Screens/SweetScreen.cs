using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Screens
{
    public partial class SweetScreen : Screen
    {
        protected SweetScreenStack ScreenStack => (SweetScreenStack)Parent;

        public bool IsSubScreenOpen => ScreenStack.IsSubScreenOpen;

        public SweetScreen()
        {
            Anchor = Origin = Anchor.Centre;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (ScreenStack is not { IsSubScreenOpen: true }) return base.OnExiting(e);

            ScreenStack.ExitSubScreen();
            return true;
        }

        /// <summary>
        /// Calls <see cref="SweetScreenStack.Exit"/>, if a <see cref="SweetSubScreen"/> is open, the current screen will handle exiting the current sub screen.
        /// </summary>
        protected void Exit() => ScreenStack.Exit();

        /// <summary>
        /// Calls <see cref="SweetScreenStack.Push"/>
        /// </summary>
        /// <param name="screen">The new screen to add</param>
        protected void Push(SweetScreen screen) => ScreenStack.Push(screen);

        /// <summary>
        /// Calls <see cref="SweetScreenStack.PushSynchronously"/>
        /// </summary>
        /// <param name="screen">The screen to push synchronously</param>
        protected void PushSynchronously(SweetScreen screen) => ScreenStack.PushSynchronously(screen);

        /// <summary>
        /// Calls <see cref="SweetScreenStack.PushSubScreen"/>
        /// </summary>
        /// <param name="subScreen">The sub screen to push into the sub stack</param>
        protected void PushSubScreen(SweetSubScreen subScreen) => ScreenStack.PushSubScreen(subScreen);

        /// <summary>
        /// Calls <see cref="SweetScreenStack.ExitSubScreen"/>
        /// <para/>
        /// Another way to exit the SubScreen, if managed properly you can just use <see cref="Exit"/>.
        /// </summary>
        protected void ExitSubScreen() => ScreenStack.ExitSubScreen();
    }
}
