using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Threading;

namespace LivinOnSweets.API.Components
{
    // https://github.com/ppy/osu/blob/master/osu.Game/OsuGame.cs#L1378
    // Not exactly a component, because we need access to the Scheduler, LoadCompAsync and dependencies, soo
    public partial class SingleThreadLoad : CompositeDrawable
    {
        private Task asyncLoadStream;
        private DependencyContainer dependencies;

        /// <summary>
        /// Queues loading the provided component in sequential fashion.
        /// This operation is limited to a single thread to avoid saturating all cores.
        /// </summary>
        /// <param name="component">The component to load.</param>
        /// <param name="loadCompleteAction">An action to invoke on load completion (generally to add the component to the hierarchy).</param>
        /// <param name="cache">Whether to cache the component as type <typeparamref name="T"/> into the game dependencies before any scheduling.</param>
        public T ScheduleLoad<T>(T component, Action<Drawable> loadCompleteAction, bool cache = false)
            where T : class
        {
            if (cache)
                dependencies.CacheAs(component);

            Drawable drawableComponent = component as Drawable ??
                                            throw new ArgumentException($"Component must be a {nameof(Drawable)}", nameof(component));

            Schedule(() =>
            {
                Task previousLoadStream = asyncLoadStream;

                asyncLoadStream = Task.Run(async () =>
                {
                    if (previousLoadStream != null)
                        await previousLoadStream.ConfigureAwait(false);

                    try
                    {
                        Logger.Log($"[SingleThreadLoad] Loading {component}");

                        Task task = null;
                        ScheduledDelegate del = new ScheduledDelegate(() =>
                            task = LoadComponentAsync(drawableComponent, loadCompleteAction));
                        Scheduler.Add(del);

                        while (!IsDisposed && !del.Completed)
                            await Task.Delay(10).ConfigureAwait(false);

                        if (IsDisposed)
                            return;

                        Debug.Assert(task != null);

                        await task.ConfigureAwait(false);

                        Logger.Log($"[SingleThreadLoad] Loaded {component}");
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
            });

            return component;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }
}
