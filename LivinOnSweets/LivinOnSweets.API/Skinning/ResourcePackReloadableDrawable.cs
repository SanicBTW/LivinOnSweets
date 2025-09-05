using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Threading;

namespace LivinOnSweets.API.Skinning
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Skinning/SkinReloadableDrawable.cs
    /// <summary>
    /// A poolable drawable implementation which has a pre-wired callback (see <see cref="PackChanged"/>) that fires
    /// once on load and again on any subsequent resource pack change.
    /// </summary>
    public abstract partial class ResourcePackReloadableDrawable : PoolableDrawable
    {
        private ScheduledDelegate pendingPackChange;

        /// <summary>
        /// Invoked when <see cref="CurrentPack"/> has changed.
        /// </summary>
        public event Action OnPackChanged;

        /// <summary>
        /// The current resource pack source.
        /// </summary>
        protected IResourcePackSource CurrentPack { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource source)
        {
            CurrentPack = source;
            CurrentPack.SourceChanged += onChange;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            packChanged();
        }

        /// <summary>
        /// Force any pending <see cref="OnPackChanged"/> calls to be performed immediately.
        /// </summary>
        /// <remarks>
        /// When a resource pack change occurs, the handling provided by this class is scheduled.
        /// In some cases, such a sample playback, this can result in the sample being played
        /// just before it is updated to a potentially different sample.
        ///
        /// Calling this method will ensure any pending update operations are run immediately.
        /// It is recommended to call this before consuming the result of resource pack changes for anything non-drawable.
        /// </remarks>
        protected void FlushPendingPackChanges()
        {
            if (pendingPackChange == null)
                return;

            pendingPackChange.RunTask();
            pendingPackChange = null;
        }

        /// <summary>
        /// Called when a change is made to the resource pack.
        /// </summary>
        /// <param name="newPack">The new resource pack.</param>
        protected virtual void PackChanged(IResourcePackSource newPack) { }

        private void onChange()
        {
            // schedule required to avoid calls after disposed.
            // note that this has the side-effect of components only performing a resource pack change when they are alive.
            pendingPackChange?.Cancel();
            pendingPackChange = Scheduler.Add(packChanged);
        }

        private void packChanged()
        {
            PackChanged(CurrentPack);
            OnPackChanged?.Invoke();

            pendingPackChange = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (CurrentPack.IsNotNull())
                CurrentPack.SourceChanged -= onChange;

            OnPackChanged = null;
        }
    }
}
