using JetBrains.Annotations;
using LivinOnSweets.API.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Components
{
    /// <summary>
    /// Simple component which manages the ownership of the <see cref="GameView"/> for the "embedded" game.
    /// </summary>
    [Cached(typeof(GameSession))] // cache this for our children
    public partial class GameSession : CompositeDrawable
    {
        [Resolved] private GameHost host { get; set; }
        [Resolved] private GameStateManager stateManager { get; set; }
        [CanBeNull] private IFocusManager focusManager { get; set; }

        private Logger logger;
        private GameView gameView;

        private readonly Bindable<Type> ownerType = new(); // not used for anything in the logic, only for displaying the current owner inside the GameSessionExplorer
        private OwnershipRequest? currentRequest;
        private int transferToken; // only for tracking the amount of transferations done in the current runtime
        private bool transferInProgress;
        private readonly Queue<OwnershipRequest> queue = new();

        /// <summary>
        /// A <see cref="BindableBool"/> to enable the input propagation into <see cref="gameView"/>.
        /// </summary>
        public readonly BindableBool InputEnabled = new();

        [BackgroundDependencyLoader]
        private void load()
        {
            GameView.Disposed += gameDisposed;
            logger = Logger.GetLogger("GameSession");
            initNload();
        }

        public void RequestOwnership(Type requester, [CanBeNull] Container newOwner, Action<GameView> onResolved = null, Action lostOwnership = null)
        {
            ownerType.Value = requester;

            logger.Add($"{requester} is requesting the ownership [#{transferToken++}, will fire an action when losing ownership? {lostOwnership != null}]");
            queue.Enqueue(new OwnershipRequest(newOwner, onResolved, lostOwnership));
            scheduleOwnershipChange();
        }

        public void ReleaseOwnership(Action finishedAction = null)
        {
            logger.Add($"Queueing the release of the ownership. [#{transferToken++}, will fire an action after releasing? {finishedAction != null}]");
            queue.Enqueue(new OwnershipRequest(null, _ => finishedAction?.Invoke(), null)); // why would we want to fire a callback when we lost the game here
            scheduleOwnershipChange();
        }

        // leaving this here just in case i come back someday but its pretty broken, losing the focus on a click or just losing it altogether
        public bool RequestGameFocus(bool focus = true)
        {
            logger.Add($"Requesting game focus ({focus})");
            Drawable target = focus ? gameView : null;
            bool focusRes = focusManager?.ChangeFocus(target) ?? false;
            /* this only works when the game view "requests focus" and "accepts it"
            if (focusRes && target != null)
                focusManager?.TriggerFocusContention(target);*/
            return focusRes;
        }

        private void scheduleOwnershipChange()
        {
            if (transferInProgress) return;

            transferInProgress = true;
            host.UpdateThread.Scheduler.Add(processQueue);
        }

        private void processQueue()
        {
            if (queue.Count == 0)
            {
                transferInProgress = false;
                return;
            }

            OwnershipRequest request = queue.Dequeue();
            if (currentRequest.HasValue)
            {
                currentRequest.Value.Target?.Remove(gameView, false);
                currentRequest.Value.LostOwnership?.Invoke();
            }

            request.Target?.Add(gameView);
            request.Callback?.Invoke(gameView);

            currentRequest = request;

            transferInProgress = false;
            scheduleOwnershipChange();
        }

        private void gameDisposed()
        {
            if (host.ExecutionState != ExecutionState.Running)
                return;

            gameView = null;
            currentRequest = null;
            initNload();
        }

        // bruh
        private void initNload()
        {
            stateManager.GameplayMachine.CurrentState.SetDefault(); // for good measure, we reset the gameplay state before creating/disposing the game
            LoadComponent(gameView = new GameView());
            logger.Add($"Loaded {nameof(gameView)} synchronously with execution time of {Time.Current:0.00}s.");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            focusManager = GetContainingFocusManager();
        }

        private readonly struct OwnershipRequest(
            Container target,
            Action<GameView> callback,
            Action lostOwnership)
        {
            [CanBeNull] public readonly Container Target = target;
            [CanBeNull] public readonly Action<GameView> Callback = callback;
            [CanBeNull] public readonly Action LostOwnership = lostOwnership;
        }
    }
}
