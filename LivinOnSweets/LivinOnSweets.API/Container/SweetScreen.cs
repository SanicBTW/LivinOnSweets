using osu.Framework.Screens;

namespace LivinOnSweets.API.Container
{
    public partial class SweetScreen : Screen
    {
        // 100% Sure that the parent is a screen stack (since it will throw otherwise)
        protected ScreenStack ScreenStack => (ScreenStack)Parent;
    }
}
