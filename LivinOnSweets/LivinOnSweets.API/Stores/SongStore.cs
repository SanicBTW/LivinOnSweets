using System.Reflection;
using LivinOnSweets.API.Data.Song;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Stores
{
    public class SongStore
    {
        private readonly Storage cache;
        private readonly AudioManager audio;

        private readonly List<ISongStore> stores = [];
        private readonly List<ISongConverter> converters = [];

        // Saves a reference to the resource stores saved in TrackStore and SampleStore
        private readonly ResourceStore<byte[]> backingTrackStore;
        private readonly ResourceStore<byte[]> backingSampleStore;

        public SongStore(Storage cacheStorage, AudioManager audioManager)
        {
            cache = cacheStorage;
            audio = audioManager;

            // The reason why we do this is because using GetTrackStore CREATES a new TrackStore with the provided ResourceStore
            // Making us having to access a saved variable of it while in other cases we MIGHT want to access the resources
            // through another place, for example injecting ITrackStore to BDL, since its not possible natively
            // we have to access the underlying resource stores to add our own stores to them
            ITrackStore trackStore = audioManager.Tracks;
            backingTrackStore = (ResourceStore<byte[]>)trackStore.GetType().GetField("store", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(trackStore);

            ISampleStore sampleStore = audioManager.Samples;
            backingSampleStore = (ResourceStore<byte[]>)sampleStore.GetType().GetField("store", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(sampleStore);
        }

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

        public SongChart GetChart(string songId, string difficulty, string format = LocalSongStore.FORMAT)
        {
            lock (stores)
            {
                foreach (ISongStore store in stores)
                {
                    if (store.Format != format)
                        continue; // Only search on stores that support the format

                    Stream chartStream = store.GetChart(songId, difficulty);
                    if (chartStream == null)
                        continue;

                    lock (converters)
                    {
                        foreach (ISongConverter converter in converters)
                        {
                            if (converter.Format != format)
                                continue; // Only search for the converter that supports the chart format

                            // Call the conversion and if needed the converter will save the converted output in the cache storage
                            return converter.ConvertChart(chartStream, cache);
                        }
                    }
                }
            }

            // should return a test chart? instead of crashing or sum tbh
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
                return stores.Where(store => store.Format == format);
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
            {
                stores.Add(store);
                store.RegisterAudioStores(backingTrackStore, backingSampleStore);
            }
        }

        public void RemoveStore(ISongStore store)
        {
            lock (stores)
            {
                store.UnregisterAudioStores(backingTrackStore, backingSampleStore);
                stores.Remove(store);
            }
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
