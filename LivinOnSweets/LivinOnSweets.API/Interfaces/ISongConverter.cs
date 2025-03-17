using LivinOnSweets.API.Data.Song;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Interfaces
{
    public interface ISongConverter
    {
        string Format { get; }

        // Converts the chart to the native format
        // Will ALWAYS save the converted chart to the cache directory
        SongChart ConvertChart(Stream input, Storage cacheStorage);
    }
}
