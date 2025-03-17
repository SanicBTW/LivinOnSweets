using LivinOnSweets.API.Data.Song;
using LivinOnSweets.API.Interfaces;
using osu.Framework.IO.Stores;
using Tomlyn;

namespace LivinOnSweets.API.Stores
{
    // Represents the local song store (Resources)
    // I kinda hate it being on the same namespace (folder) but whatever
    public class LocalSongStore : ISongStore
    {
        // Livin Chart Format v1
        public const string FORMAT = "LCFv1";

        // Targetting the native (game) chart format
        public string Format => FORMAT;

        private PreservingNamespaceResourceStore<byte[]> backingResStore;

        // id / metadata, will be saved upon getmeta call
        private Dictionary<string, SongMetadata> metaCache = [];

        public LocalSongStore(ResourceStore<byte[]> resources)
        {
            backingResStore = new PreservingNamespaceResourceStore<byte[]>(resources, "Songs");
        }

        public SongMetadata GetMetadata(string songId)
        {
            if (!metaCache.TryGetValue(songId, out SongMetadata meta))
            {
                using Stream stream = backingResStore.GetStream($"Songs/{songId}/metadata.toml");
                using StreamReader reader = new StreamReader(stream);
                string content = reader.ReadToEnd();

                metaCache[songId] = meta = Toml.ToModel<SongMetadata>(content);
            }

            return meta;
        }

        public Stream GetChart(string songId, string difficulty)
        {
            // we using the meta/cached meta, to index the difficulties, if none exist then we check inside the same directory
            // if we still cant find the chart, call it a fail
            SongMetadata meta = GetMetadata(songId);

            string chartPath;

            // should analyze the chart path to see if its an absolute dir or something
            // to properly open the stream
            if (!meta.Charts.TryGetValue(difficulty, out chartPath))
                throw new NotImplementedException();

            if (chartPath.Length <= 0)
            {
                // hardcoded for now since im too lazy (2:16 am)
                // should make the difficulty title cased bruh (2:21 am)
                // I forgot the .net resource name rules, i'll think about a good file name
                // or even better! just leave it as the difficulty bruh (2:23 am)
                // TODO: Implement dynamic functionality
                chartPath = $"Songs/{songId}/{difficulty}.toml";
            }

            Stream stream = backingResStore.GetStream(chartPath);
            return stream;
        }

        // This hides Songs\ and the toml search target, the reason why we targetting the toml files
        // is because theres only 4 of them, making the enumeration easier
        public IEnumerable<string> GetAvailableSongs() =>
            backingResStore.GetAvailableResources()
                .Where(s => s.EndsWith(".toml"))
                .Select(s => Path.GetFileName(Path.GetDirectoryName(s)));

        // Refer to AlbumSprite to know how to use the registered store
        void ISongStore.RegisterAudioStores(ResourceStore<byte[]> trackStore, ResourceStore<byte[]> sampleStore) =>
            trackStore.AddStore(backingResStore);

        void ISongStore.UnregisterAudioStores(ResourceStore<byte[]> trackStore, ResourceStore<byte[]> sampleStore) =>
            trackStore.RemoveStore(backingResStore);
    }
}
