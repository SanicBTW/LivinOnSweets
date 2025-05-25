using Android.OS;
using Android.Window;
using JetBrains.Annotations;
using LivinOnSweets.API.Input;
using LivinOnSweets.Game;
using osu.Framework.Allocation;

namespace LivinOnSweets.Android
{
    public partial class SweetGameAndroid(SweetGameActivity activity) : LivinOnSweetsGame
    {
        [Cached]
        private readonly SweetGameActivity gameActivity = activity;

        // Should expose this in some way or other for specific cases to keep the screen active
        // Will probably make an event for it with AetherFramework most likely
        [CanBeNull] private PowerManager.WakeLock activeWakeLock;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // To keep the screen active but dimmed, behaviour will change in future versions
            // only creating the wakelock to keep the game active in specific screens
            activeWakeLock = SweetGameActivity.AcquireWakeLock();
            activeWakeLock!.Acquire();

            // i only found a way for Android API 33 (Android 13) once I find a way for older APIs i'll implement it
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                gameActivity.OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(100, new BackActionDispatcher(ActionContainer));

            // FORCED to do this, set the orientation (once again), since for some reason heres the place
            // to make the screen rotate, while the activity holds the first rotation, the next changes
            // will make the screen rotate to vertical... kinda weird (also includes a quick orientation change for some reason, no idea)
            gameActivity.RequestedOrientation = gameActivity.DefaultOrientation;
        }

        private partial class BackActionDispatcher(ManiaActionContainer maniaActionContainer)
            : Fragment, IOnBackInvokedCallback
        {
            public void OnBackInvoked()
            {
                maniaActionContainer.TriggerPressed(ManiaAction.BACK);
            }
        }
    }
}
