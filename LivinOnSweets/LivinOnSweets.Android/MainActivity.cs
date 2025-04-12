using Android.Views;
using LivinOnSweets.Game;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;

namespace LivinOnSweets.Android
{
    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, Label = "LIVIN' ON SWEETS!", MainLauncher = true)]
    public class MainActivity : AndroidGameActivity
    {
        protected override osu.Framework.Game CreateGame() => new LivinOnSweetsGame();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.HardwareAccelerated);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);
        }
    }
}
