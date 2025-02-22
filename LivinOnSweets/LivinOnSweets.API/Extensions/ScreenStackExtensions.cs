using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Extensions
{
    public static class ScreenStackExtensions
    {
        private static MethodInfo removeInternal =
            typeof(CompositeDrawable).GetMethod("RemoveInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Remove(this ScreenStack screenStack, Drawable drawable, bool disposeImmediately) =>
            removeInternal?.Invoke(screenStack, [ drawable, disposeImmediately ]);
    }
}
