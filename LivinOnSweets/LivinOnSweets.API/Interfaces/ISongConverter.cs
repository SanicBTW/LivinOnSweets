using osu.Framework.Platform;

namespace LivinOnSweets.API.Interfaces
{
    public interface ISongConverter
    {
        string Format { get; }

        // If the chart format should be converted
        bool CanConvert(string format);

        // Converts the chart to the native format
        // Will ALWAYS save the converted chart to the cache directory
        object ConvertChart(object input, Storage cacheStorage);
    }
}
