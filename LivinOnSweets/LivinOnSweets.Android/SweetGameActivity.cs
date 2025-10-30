using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;
using Uri = Android.Net.Uri;
// ReSharper disable MemberCanBePrivate.Global

namespace LivinOnSweets.Android
{
    // Kinda copied osu!lazer android code, but they know best
    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, Label = "LIVIN' ON SWEETS!", MainLauncher = true, ScreenOrientation = ScreenOrientation.UserLandscape)]
    public class SweetGameActivity : AndroidGameActivity
    {
        public const string SHARED_PREFERENCES = "v1_los_android_settings";
        public const string UGC_FOLDER_URI = "ugc_folder_uri";
        public const int PICK_USER_FOLDER = 0x02;

        private const string wake_lock_tag = "SweetGameActivity::WakeLock";

        /// <summary>
        /// The default screen orientation.
        /// </summary>
        public ScreenOrientation DefaultOrientation = ScreenOrientation.Unspecified;

        protected override void Main() => new SweetAndroidHost(this).Run(CreateGame());

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

            // uhh should make a screen that lets the user know that it needs to pick a custom folder to dump the resource packs n shi into
            requestUserFolder();
        }

        private void requestUserFolder()
        {
            ISharedPreferences prefs = GetPreferences();
            if (prefs.Contains(UGC_FOLDER_URI))
            {
                SweetAndroidHost.CustomUserFolder = prefs.GetString( UGC_FOLDER_URI, null);
                return;
            }

            Intent intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantPersistableUriPermission);
            StartActivityForResult(intent, PICK_USER_FOLDER);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PICK_USER_FOLDER && resultCode == Result.Ok)
            {
                Uri treeUri = data?.Data;
                if (treeUri == null)
                {
                    // TODO : should fallback properly
                    return;
                }

                ContentResolver?.TakePersistableUriPermission(treeUri,
                    ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                string treeUriStr = treeUri.ToString();
                GetPreferences().Edit()?.PutString(UGC_FOLDER_URI, treeUriStr)!.Apply();
                SweetAndroidHost.CustomUserFolder = treeUriStr;
            }
        }

        // da funny help function, probably not the best thing buttt reusable i believe
        public static PowerManager.WakeLock AcquireWakeLock()
        {
            PowerManager powerManager =
                (PowerManager)Context?.GetSystemService(PowerService);
            PowerManager.WakeLock wakeLock = powerManager?.NewWakeLock(WakeLockFlags.ScreenDim, wake_lock_tag);
            return wakeLock;
        }

        public static ISharedPreferences GetPreferences()
        {
            ISharedPreferences prefs = Context?.GetSharedPreferences(SHARED_PREFERENCES, FileCreationMode.Private);
            return prefs;
        }
    }
}
