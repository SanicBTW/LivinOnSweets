using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Containers
{
    public partial class SweetScreen : Screen
    {
        private static FieldInfo loadStateFieldInf =
            typeof(Drawable).GetField("loadState", BindingFlags.Instance | BindingFlags.NonPublic);

        // 100% Sure that the parent is a screen stack (since it will throw otherwise)
        protected ScreenStack ScreenStack => (ScreenStack)Parent;

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        // Internal because its highly unsafe to do this
        // I'm only using it to be able to change the screenstack where the screen was added to
        internal void ChangeLoadState(LoadState newState)
        {
            loadStateFieldInf.SetValue(this, newState);
            if (LoadState != newState)
                throw new InvalidOperationException("Reflection failed");
        }
    }
}
