using Android.OS;
using JetBrains.Annotations;
using LivinOnSweets.Game;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK;

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

            // FORCED to do this, set the orientation (once again), since for some reason heres the place
            // to make the screen rotate, while the activity holds the first rotation, the next changes
            // will make the screen rotate to vertical... kinda weird (also includes a quick orientation change for some reason, no idea)
            gameActivity.RequestedOrientation = gameActivity.DefaultOrientation;
        }
    }
}
