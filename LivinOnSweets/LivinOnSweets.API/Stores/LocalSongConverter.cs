using LivinOnSweets.API.Data.Song;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Platform;
using Tomlyn;

namespace LivinOnSweets.API.Stores
{
    public class LocalSongConverter : ISongConverter
    {
        public string Format => LocalSongStore.FORMAT;

        // Returns a LCFv1 standard chart format
        public SongChart ConvertChart(Stream input, Storage cacheStorage)
        {
            using StreamReader reader = new StreamReader(input);
            string content = reader.ReadToEnd();

            return Toml.ToModel<SongChart>(content);
        }
    }
}
