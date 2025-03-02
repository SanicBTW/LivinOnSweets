using LivinOnSweets.API.Data.Song;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Stores
{
    public class SongStore
    {
        private readonly Storage cache;

        private readonly List<ISongStore> stores = [];
        private readonly List<ISongConverter> converters = [];

        public SongStore(Storage cacheStorage)
        {
            cache = cacheStorage;
        }

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

        public object GetChart(string songId, string difficulty, string format = LocalSongStore.FORMAT)
        {
            lock (stores)
            {
                foreach (ISongStore store in stores)
                {
                    if (format != LocalSongStore.FORMAT && store.Format != format)
                        continue; // Only search on stores that support the format

                    object chartStream = store.GetChart(songId, difficulty);
                    if (chartStream == null)
                        continue;

                    if (format == LocalSongStore.FORMAT)
                        return chartStream;

                    lock (converters)
                    {
                        foreach (ISongConverter converter in converters)
                        {
                            if (!converter.CanConvert(format))
                                continue;

                            return converter.ConvertChart(chartStream, cache);
                        }
                    }
                }
            }

            return null;
        }

        // Enumeration

        public IEnumerable<string> GetAvailableSongs()
        {
            lock (stores)
                return stores.SelectMany(s => s.GetAvailableSongs());
        }

        public IEnumerable<string> GetAvailableSongsByFormat(string format = LocalSongStore.FORMAT)
        {
            lock (stores)
            {
                return stores
                    .Where(store => store.Format == format)
                    .SelectMany(store => store.GetAvailableSongs())
                    .Distinct();
            }
        }

        public IEnumerable<ISongStore> GetStoresByFormat(string format)
        {
            lock (stores)
                return stores.Where(store => store.Format == format).ToList();
        }

        // I don't think you would ever need to get a converter
        public IEnumerable<string> GetAvailableConverters()
        {
            lock (converters)
                return converters.Select(s => s.Format);
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

        public void AddConverter(ISongConverter converter)
        {
            lock (converters)
                converters.Add(converter);
        }

        public void RemoveConverter(ISongConverter converter)
        {
            lock (converters)
                converters.Remove(converter);
        }
    }
}
