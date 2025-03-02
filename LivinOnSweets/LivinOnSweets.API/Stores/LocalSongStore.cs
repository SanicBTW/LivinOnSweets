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

        public object GetChart(string songId, string difficulty)
        {
            throw new NotImplementedException();
        }

        // This hides Songs\ and the toml search target, the reason why we targetting the toml files
        // is because theres only 4 of them, making the enumeration easier
        public IEnumerable<string> GetAvailableSongs() =>
            backingResStore.GetAvailableResources()
                .Where(s => s.EndsWith(".toml"))
                .Select(s => Path.GetFileName(Path.GetDirectoryName(s)));
    }
}
