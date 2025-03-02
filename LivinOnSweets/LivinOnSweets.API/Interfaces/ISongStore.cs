using LivinOnSweets.API.Data.Song;

namespace LivinOnSweets.API.Interfaces
{
    public interface ISongStore
    {
        string Format { get; }

        // Returns the TOML Metadata
        SongMetadata GetMetadata(string songId);

        // Implement soon
        object GetChart(string songId, string difficulty);

        // Returns a list of ids
        IEnumerable<string> GetAvailableSongs();
    }
}
