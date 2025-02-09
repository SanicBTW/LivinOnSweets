using osu.Framework.Screens;

namespace LivinOnSweets.API.Containers
{
    public partial class SweetScreen : Screen
    {
        // 100% Sure that the parent is a screen stack (since it will throw otherwise)
        protected ScreenStack ScreenStack => (ScreenStack)Parent;

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }
    }
}
