using LivinOnSweets.API.Song;
using LivinOnSweets.ChartFormat;
using LivinOnSweets.ChartFormat.Adapters;
using LivinOnSweets.ChartFormat.Models;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Interfaces
{
    /// <summary>
    /// Interface used to define custom Song Stores.
    /// </summary>
    public interface ISongStore
    {
        /// <summary>
        /// The target format of the Song Store.
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Looks for <paramref name="songId"/> inside the stores and returns the <see cref="SongMetadata"/> of the song.
        /// </summary>
        /// <param name="songId">The song id to lookup in the stores.</param>
        /// <returns>The <see cref="SongMetadata"/> of <paramref name="songId"/>.</returns>
        SongMetadata GetMetadata(string songId);

        /// <summary>
        /// Fetches a <see cref="ChartData"/> adapted from this <see cref="ISongStore"/> <see cref="Format"/>. (if needed)
        /// </summary>
        /// <param name="songId">The song id to lookup in the stores.</param>
        /// <param name="difficulty">The song difficulty.</param>
        /// <remarks>
        /// The intended way to use this is by registering a <see cref="IChartAdatper{TExternal}"/>
        /// and using the <see cref="ChartPipeline"/> to fetch the adapted chart from this <see cref="Format"/>.
        /// </remarks>
        /// <returns>A fresh <see cref="ChartData"/> ready for usage.</returns>
        ChartData GetChart(string songId, string difficulty);

        /// <summary>
        /// Fetch the available songs on this <see cref="ISongStore"/>.
        /// </summary>
        /// <returns>A list of available song IDs.</returns>
        IEnumerable<string> GetAvailableSongs();

        /// <summary>
        /// Retrieves the <see cref="ISongStore"/>'s scoped cache storage.
        /// </summary>
        /// <returns>A <see cref="Storage"/> object.</returns>
        Storage GetCacheStorage();
    }
}
