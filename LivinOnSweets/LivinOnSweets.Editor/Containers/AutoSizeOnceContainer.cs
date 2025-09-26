using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace LivinOnSweets.Editor.Containers
{
    // honestly im not too happy with this code and its usage, pretty sure theres plenty of other ways to make the size dynamic without too much resizes
    public partial class AutoSizeOnceContainer(Axes disableOnAxis) : AutoSizeOnceContainer<Drawable>(disableOnAxis) { }

    // A basic container which sets AutoSize axes to both on creation and after one UpdateAfterAutoSize set it to None
    public partial class AutoSizeOnceContainer<T>(Axes disableOnAxis) : Container<T>
        where T : Drawable
    {
        // Quick flag to avoid using HasFlagFast each auto size call
        private bool applied;

        // The axis to remove from AutoSizeAxes on the first auto size call

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

            // If applied, or not targetting to any axes or doesn't have auto size axes, return
            if (applied || disableOnAxis == Axes.None || AutoSizeAxes == Axes.None)
                return;

            // Original comment from Editor/Toolbar.cs
            // This disables the auto sizing on the specified axis while also keeping the size before disabling the auto sizing
            // this is made to be able to modify the sprites without worrying about the size of this container changing
            applied = true;
            if (AutoSizeAxes.HasFlagFast(disableOnAxis))
            {
                Vector2 prevSize = DrawSize;
                AutoSizeAxes &= ~disableOnAxis;

                switch (disableOnAxis)
                {
                    case Axes.Both:
                        Size = prevSize;
                        break;

                    case Axes.X:
                        Width = prevSize.X;
                        break;

                    case Axes.Y:
                        Height = prevSize.Y;
                        break;
                }
            }
        }
    }
}
