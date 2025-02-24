using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace LivinOnSweets.API.Extensions
{
    public static class CompositeDrawableExtensions
    {
        private static MethodInfo removeInternal =
            typeof(CompositeDrawable).GetMethod("RemoveInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        // Changed from ScreenStack to CompositeDrawable, since ScreenStack inherits from it
        public static void Remove(this CompositeDrawable screenStack, Drawable drawable, bool disposeImmediately) =>
            removeInternal?.Invoke(screenStack, [ drawable, disposeImmediately ]);
    }
}
