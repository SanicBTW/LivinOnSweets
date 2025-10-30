using Android.Content;
using Android.Provider;
using AndroidX.DocumentFile.Provider;
using JetBrains.Annotations;
using osu.Framework.Android;
using osu.Framework.Platform;
using Uri = Android.Net.Uri;

namespace LivinOnSweets.Android
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class SweetAndroidHost(AndroidGameActivity activity) : AndroidGameHost(activity)
    {
        // due to the nature of document file.fromtreeuri it will always return to primary:<selectedfolder> and wont begin from any other path, so to avoid allocating the same fucky wucky we just cache it
        internal static DocumentFile DirectoryDoc;
        internal static Uri UserFolderUri;
        private static string customUserFolder;
        [CanBeNull]
        internal static string CustomUserFolder
        {
            get => customUserFolder;
            set
            {
                if (customUserFolder == value)
                    return;

                customUserFolder = value;
                UserFolderUri ??= Uri.Parse(value);
                DirectoryDoc ??= DocumentFile.FromTreeUri(Application.Context, UserFolderUri);

                if (DirectoryDoc != null)
                    SweetStorage.BuildCache();
            }
        }

        private readonly AndroidGameActivity activity = activity;

        public override Storage GetStorage(string path)
            => CustomUserFolder != null &&
                path.StartsWith(CustomUserFolder, StringComparison.OrdinalIgnoreCase) ?
                    new SweetStorage(path, activity.ApplicationContext!)
                    :
                    new AndroidStorage(path, this);

        public override IEnumerable<string> UserStoragePaths => CustomUserFolder != null ? [
            base.UserStoragePaths.First(),
            CustomUserFolder
        ] : base.UserStoragePaths;
    }
}
