using Android.Content;
using AndroidX.DocumentFile.Provider;
using JetBrains.Annotations;
using osu.Framework.Platform;
using Uri = Android.Net.Uri;

namespace LivinOnSweets.Android
{
    // god i should really stop calling everything sweet, theres nothing sweet on this code damn
    // heard that document file has a lot of overhead, if the performance hit is very clear ill work on manual stuff by myself, but until then theres still a lot of time ahead
    // sanco the next day: ok uhh i implemented some cascading cache which caches the lookups for the next directories, should only be called once since it cascades recursively over the paths
    // still its not ideal over big and large folders but oh well its only done like once
    // sanco on commit time: i know, i know this code can improve A LOT but it is what works properly for me and i took 12 hours to work on, if you have any improvements, please feel free to contribute them
    public class SweetStorage(string path, [CanBeNull] Context context, [CanBeNull] Uri treeUri = null)
        : Storage(path)
    {
        // cache should be built already when creating a new sweet storage since build cache its called when setting the uri on sweet android host
        private static bool cacheBuilt;
        private static readonly object cache_lock = new();
        private static readonly Dictionary<string, DocumentFile[]> directory_cache = [];
        private static readonly Dictionary<string, DocumentFile[]> file_cache = [];

        private readonly Uri funcUri = treeUri ?? SweetAndroidHost.UserFolderUri;

        public override Storage GetStorageForDirectory(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (path.Length > 0 && !path.EndsWith(Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

            string fullPath = GetFullPath(path, true);

            // since paths are now relative, we want to remove misleading separators since the base path will get combined on certain calls
            Uri nextUri = Uri.Parse(fullPath);
            return (Storage)Activator.CreateInstance(GetType(), path.TrimEnd(Path.DirectorySeparatorChar), context, nextUri);
        }

        public override bool Exists(string path)
        {
            return getDocumentFile(path)?.Exists() ?? false;
        }

        public override bool ExistsDirectory(string path)
        {
            return getDocumentFile(path)?.IsDirectory ?? false;
        }

        public override void DeleteDirectory(string path)
        {
            getDocumentFile(path)?.Delete();
        }

        public override void Delete(string path)
        {
            getDocumentFile(path)?.Delete();
        }

        public override void Move(string from, string to)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetFiles(string path, string pattern = "*")
        {
            string fullPath = Path.Combine(BasePath, path);
            if (!file_cache.TryGetValue(fullPath, out var files))
                return [];

            IEnumerable<DocumentFile> selectedFiles = files;

            if (pattern != "*")
            {
                // not using regexes because i dont want to, already calling listFiles causes overhead and using the funny regexes would be a crazy mix
                // so we just doing some string comparison i guess, only tried it on the sweet assembly engine so i cant be for sure if it meets all the pattern filtering standards?
                string lowerPattern = pattern.ToLowerInvariant();
                bool startsWithStar = pattern.StartsWith("*");
                bool endsWithStar = pattern.EndsWith("*");
                string trimmedPattern = pattern.Trim('*').ToLowerInvariant();

                selectedFiles = files.Where(f =>
                {
                    string name = f.Name!;
                    if (!startsWithStar && !endsWithStar)
                        return string.Equals(name, lowerPattern, StringComparison.CurrentCultureIgnoreCase);

                    if (startsWithStar && endsWithStar)
                        return name.Contains(trimmedPattern, StringComparison.CurrentCultureIgnoreCase);

                    if (startsWithStar)
                        return name.EndsWith(trimmedPattern, StringComparison.CurrentCultureIgnoreCase);

                    return name.StartsWith(trimmedPattern, StringComparison.CurrentCultureIgnoreCase);
                });
            }

            IEnumerable<string> ret =
                selectedFiles.Select(f => string.IsNullOrEmpty(path) ? f.Name! : Path.Combine(path, f.Name!));
            return ret;
        }

        public override IEnumerable<string> GetDirectories(string path)
        {
            string fullPath = Path.Combine(BasePath, path);
            if (!directory_cache.TryGetValue(fullPath, out var dirs))
                return [];

            IEnumerable<string> ret =
                dirs.Select(d => string.IsNullOrEmpty(path) ? d.Name! : Path.Combine(path, d.Name!));
            return ret;
        }

        public override string GetFullPath(string path, bool createIfNotExisting = false)
        {
            if (string.IsNullOrEmpty(path))
                return BasePath;

            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (createIfNotExisting)
                createDirectory(path);

            return Path.Combine(BasePath, path);
        }

        public override bool OpenFileExternally(string filename) => false;

        public override bool PresentFileExternally(string filename) => false;

        public override Stream GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate)
        {
            DocumentFile file = getDocumentFile(path);
            if (file == null)
                return null;

            if (access == FileAccess.Read)
                return context!.ContentResolver!.OpenInputStream(file.Uri!);

            return context!.ContentResolver!.OpenOutputStream(file.Uri!, access == FileAccess.Write ? "rw" : "r");
        }

        private void createDirectory(string relativePath)
        {
            relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrEmpty(relativePath))
                return;

            string[] segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            DocumentFile current = getDocumentFile("");
            if (current == null)
                return;

            foreach (string segment in segments)
            {
                DocumentFile next = current.ListFiles()?.FirstOrDefault(f => f.Name == segment && f.IsDirectory);
                if (next != null)
                {
                    current = next;
                    continue;
                }

                next = current.CreateDirectory(segment);

                if (next != null)
                {
                    int index = Array.IndexOf(segments, segment);
                    string curPath = string.Join(Path.DirectorySeparatorChar, segments.Take(index));

                    lock (cache_lock)
                    {
                        if (!directory_cache.TryGetValue(curPath, out var list))
                            list = [];

                        directory_cache[curPath] = list.Append(next).ToArray();
                    }
                }
            }
        }

        private DocumentFile getDocumentFile(string relativePath)
        {
            relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // since passing down a different uri wont change the document file root (which is supposed to change to the last folder from the uri) we just use the singleton
            DocumentFile current = SweetAndroidHost.DirectoryDoc;
            if (current == null)
                return null;

            relativePath = appendDirectoryString(relativePath);

            if (string.IsNullOrEmpty(relativePath))
                return current;

            string[] segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            string curPath = "";

            // if the first segment is base path then we could start with that and remove one iteration
            // only do this when theres enough segments to keep the for loop
            if (segments.Length > 1 && string.Equals(segments[0], BasePath, StringComparison.OrdinalIgnoreCase))
            {
                curPath = segments[0];
                segments = segments.Skip(1).ToArray();
            }

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                bool isLast = (i == segments.Length - 1);

                if (isLast)
                {
                    if (!file_cache.TryGetValue(curPath, out var files))
                        return null;

                    DocumentFile file = files.FirstOrDefault(f => f.Name == segment);
                    return file;
                }
                else
                {
                    if (!directory_cache.TryGetValue(curPath, out var dirs))
                        return null;

                    DocumentFile dir = dirs.FirstOrDefault(f => f.Name == segment);
                    if (dir == null)
                        return null;

                    current = dir;
                    curPath = string.IsNullOrEmpty(curPath) ? segment : Path.Combine(curPath, segment);
                }
            }

            return current;
        }

        private string appendDirectoryString(string relativePath)
        {
            string userFolderSegment = SweetAndroidHost.UserFolderUri.LastPathSegment;
            string targetFolder = funcUri.LastPathSegment!;

            // in case of having the same last segment for both trees, we just return the given relative path, to avoid appending
            // primary:folder and essentially getting nothing around the cache
            if (string.Equals(userFolderSegment, targetFolder, StringComparison.OrdinalIgnoreCase))
                return relativePath;

            string fullPath = funcUri.Path!;

            bool useLastSegment = fullPath.Contains(SweetAndroidHost.UserFolderUri.Path!, StringComparison.OrdinalIgnoreCase);
            string ret = Path.Combine(useLastSegment ? targetFolder : fullPath, relativePath);

            return ret;
        }

        public static void BuildCache()
        {
            if (cacheBuilt)
                return;

            // we are building the root cache so we access the singleton to avoid having to call getDocumentFile on nothing really
            DocumentFile root = SweetAndroidHost.DirectoryDoc;
            if (root == null)
                return;

            lock (cache_lock)
            {
                void recurse(DocumentFile folder, string relativePath)
                {
                    DocumentFile[] children = folder.ListFiles() ?? [];

                    directory_cache[relativePath] = children.Where(f => f.IsDirectory).ToArray();

                    file_cache[relativePath] = children.Where(f => !f.IsDirectory).ToArray();

                    foreach (DocumentFile subFolder in children.Where(f => f.IsDirectory))
                    {
                        string subPath = string.IsNullOrEmpty(relativePath) ? subFolder.Name! : Path.Combine(relativePath, subFolder.Name!);
                        recurse(subFolder, subPath);
                    }
                }

                recurse(root, "");
                cacheBuilt = true;
            }
        }
    }
}
