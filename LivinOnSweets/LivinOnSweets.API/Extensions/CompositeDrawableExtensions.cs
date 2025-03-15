using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace LivinOnSweets.API.Extensions
{
    public static class CompositeDrawableExtensions
    {
        private static MethodInfo removeInternal =
            typeof(CompositeDrawable).GetMethod("RemoveInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo addInternal =
            typeof(CompositeDrawable).GetMethod("AddInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo changeInternalChildDepth =
            typeof(CompositeDrawable).GetMethod("ChangeInternalChildDepth", BindingFlags.Instance | BindingFlags.NonPublic);


        // Changed from ScreenStack to CompositeDrawable, since ScreenStack inherits from it
        public static void RemoveInternal(this CompositeDrawable composite, Drawable drawable, bool disposeImmediately) =>
            removeInternal?.Invoke(composite, [ drawable, disposeImmediately ]);

        public static void AddInternal(this CompositeDrawable composite, Drawable drawable) =>
            addInternal?.Invoke(composite, [ drawable ]);

        public static void ChangeInternalChildDepth(this CompositeDrawable composite, Drawable drawable,
            float newDepth) =>
            changeInternalChildDepth?.Invoke(composite, [ drawable, newDepth ]);
    }
}
