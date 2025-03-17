using LivinOnSweets.API.Data.Song;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Interfaces
{
    public interface ISongStore
    {
        string Format { get; }

        // Returns the TOML Metadata
        SongMetadata GetMetadata(string songId);

        // Returns a readable Stream that can be used for parsing if needed
        Stream GetChart(string songId, string difficulty);

        // Returns a list of ids
        IEnumerable<string> GetAvailableSongs();

        // Register a resource store to the tracks/samples stores
        void RegisterAudioStores(ResourceStore<byte[]> trackStore, ResourceStore<byte[]> sampleStore);

        // Unregister the previously registered stores, in case of a dispose
        void UnregisterAudioStores(ResourceStore<byte[]> trackStore, ResourceStore<byte[]> sampleStore);
    }
}
