using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Song;
using LivinOnSweets.ChartFormat.Adapters;
using LivinOnSweets.ChartFormat.Models;

namespace LivinOnSweets.API.Stores
{
    // Previously the song store would create a global cache directory for caching the converted formats but since now
    // we are using ResourcePacks, the cache path will be scoped inside the resource pack itself
    public class SongStore
    {
        private readonly List<ISongStore> stores = [];

        // Fetching

        public SongMetadata GetMetadata(string songId)
        {
            lock (stores)
            {
                foreach (ISongStore store in stores)
                {
                    SongMetadata metadata = store.GetMetadata(songId);
                    if (metadata == null)
                        continue;

                    return metadata;
                }
            }

            return null;
        }

        public ChartData GetChart(string songId, string difficulty, string format = null)
        {
            string targetFormat = format ?? DefaultAdapter.ChartFormatName;

            lock (stores)
            {
                foreach (ISongStore store in stores)
                {
                    if (store.Format != targetFormat)
                        continue; // only search on stores that support the format

                    // the store itself will determine if the need of using the cache or calling the ChartPipeline is needed
                    return store.GetChart(songId, difficulty);
                }
            }

            // should return a test chart? instead of crashing or sum idk
            return null;
        }

        // Enumeration

        public IEnumerable<string> GetAvailableSongs()
        {
            lock (stores)
                return stores.SelectMany(s => s.GetAvailableSongs()).Distinct();
        }

        public IEnumerable<string> GetAvailableSongsByFormat(string format = null)
        {
            string targetFormat = format ?? DefaultAdapter.ChartFormatName;

            lock (stores)
            {
                return stores
                       .Where(store => store.Format == targetFormat)
                       .SelectMany(store => store.GetAvailableSongs())
                       .Distinct();
            }
        }

        public IEnumerable<ISongStore> GetStoresByFormat(string format)
        {
            lock (stores)
                return stores.Where(store => store.Format == format);
        }

        // Module list manipulation

        public void AddStore(ISongStore store)
        {
            lock (stores)
                stores.Add(store);
        }

        public void RemoveStore(ISongStore store)
        {
            lock (stores)
                stores.Remove(store);
        }
    }
}
