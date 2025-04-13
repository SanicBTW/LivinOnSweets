using Android.Content.PM;
using Android.OS;
using Android.Views;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;

namespace LivinOnSweets.Android
{
    // Kinda copied osu!lazer android code, but they know best
    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, Label = "LIVIN' ON SWEETS!", MainLauncher = true, ScreenOrientation = ScreenOrientation.UserLandscape)]
    public class SweetGameActivity : AndroidGameActivity
    {
        private const string wake_lock_tag = "SweetGameActivity::WakeLock";

        /// <summary>
        /// The default screen orientation.
        /// </summary>
        public ScreenOrientation DefaultOrientation = ScreenOrientation.Unspecified;

        protected override osu.Framework.Game CreateGame() => new SweetGameAndroid(this);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.HardwareAccelerated);
            // this doesnt really work, or its just me because im not using any music yet, but rendering alone should keep the screen active imo
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            // is a tablet taller than wider or wider than taller? why would i want to set fulluser on tablet then, idk bro
            RequestedOrientation = DefaultOrientation = ScreenOrientation.UserLandscape;
        }

        // da funny help function, probably not the best thing buttt reusable i believe
        public static PowerManager.WakeLock AcquireWakeLock()
        {
            PowerManager powerManager =
                (PowerManager)Context?.GetSystemService(PowerService);
            PowerManager.WakeLock wakeLock = powerManager?.NewWakeLock(WakeLockFlags.ScreenDim, wake_lock_tag);
            return wakeLock;
        }
    }
}
