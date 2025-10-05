using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;

namespace LivinOnSweets.API.Components
{
    // i honestly dont like this approach but its so far what works for me

    /// <summary>
    /// Simple load tracking manager, useful for tracking the progress of <see cref="Drawable"/>'s by registering
    /// them separately to get a somewhat accurate load progress.
    /// </summary>
    public partial class LoadManager : CompositeDrawable
    {
        private List<Drawable> tracking = [];
        private int total;
        private int loaded;

        public float Progress => total == 0 ? 0 : (float)loaded / total;

        public void Register<TLoadable>([NotNull] TLoadable component, Action<TLoadable> onLoaded = null, CancellationToken cancellation = default, Scheduler scheduler = null)
            where TLoadable : Drawable
        {
            register(component);

            // bruh moment
            Schedule(() => LoadComponentAsync(component, onLoaded, cancellation, scheduler));
        }

        // not my favorite thing
        protected override void Update()
        {
            List<Drawable> reg = [..tracking];

            foreach (Drawable d in tracking)
            {
                if (d.LoadState < LoadState.Ready)
                    continue;

                loaded++;
                reg.Remove(d);
            }

            tracking = reg;
        }

        private void register(Drawable d)
        {
            total++;
            tracking.Add(d);
        }
    }
}
