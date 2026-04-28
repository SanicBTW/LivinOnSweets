using JetBrains.Annotations;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Song;
using LivinOnSweets.ChartFormat;
using LivinOnSweets.ChartFormat.Adapters;
using LivinOnSweets.ChartFormat.Models;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Tomlyn;
using Tomlyn.Syntax;

namespace LivinOnSweets.API.Skinning
{
    /// <summary>
    /// An <see cref="ISongStore"/> implementation which relies on a backing <see cref="ResourcePack"/> to list out available songs.
    /// </summary>
    public class ResourcePackSongStore(ResourcePack pack, [CanBeNull] Storage storage) : ISongStore
    {
        // since we cannot access the internal resource pack store we make one based on a possible route
        // uhh quick update, after a couple of months i found a huge flaw and decided to expose the internal pack store through a readonly variable and returns the interface to avoid tampering
        private readonly IResourceStore<byte[]> resources = pack.PackStore;

        // id / metadata, will be saved upon getmeta call
        private readonly Dictionary<string, SongMetadata> metaCache = [];

        public string Format => pack.PackInfo.Songs.Format;

        public IEnumerable<string> GetAvailableSongs() => pack.PackInfo.Songs.Available;

        public SongMetadata GetMetadata(string songId)
        {
            if (!pack.IsActive)
                return null; // fall back to the next song store that its pack is active

            if (!metaCache.TryGetValue(songId, out SongMetadata metadata))
            {
                string aliasPath = getAliasedPath(songId);

                string tomlPath = $"{aliasPath}/metadata.toml";
                using Stream metaStream = resources.GetStream(tomlPath);
                if (metaStream == null)
                    return null; // falls... back?

                using StreamReader metaReader = new StreamReader(metaStream);
                string content = metaReader.ReadToEnd();

                if (!Toml.TryToModel(content, out metadata, out DiagnosticsBag diagnostics))
                {
                    pack.Logger.Add("Failed to deserialize metadata, please check the log file for more information",
                        LogLevel.Error);
                    pack.Logger.Add(diagnostics.ToString(), outputToListeners: false);
                    return null;
                }

                metaCache.TryAdd(content, metadata);
            }

            return metadata;
        }

        public ChartData GetChart(string songId, string difficulty)
        {
            // this one is hard to consider if it should fallback to the next active pack or not, since a pack can define its own improved charts, its always gonna hit on the first
            // currently with the new change, THIS part should return null and hopefully fallback to the next pack which could lead to the same behaviour really but getStream returns the stream of the current resources OR the fallback so i guess it should be fine?
            SongMetadata metadata = GetMetadata(songId);
            if (metadata == null)
                return null; // idk where tf is this gonna fall back honestly

            string aliasPath = getAliasedPath(songId);
            string chartPath = "";
            if (metadata.Charts.Count > 0)
            {
                if (!metadata.Charts.TryGetValue(difficulty, out string diffFile))
                    return null; // go for the next one?
                else
                    chartPath = $"{aliasPath}/{diffFile}";
            }

            // uhh this is fine until the format is different...
            if (string.IsNullOrWhiteSpace(chartPath))
                chartPath = $"{aliasPath}/{difficulty}.toml";

            // check if it was cached before hand (already converted)
            Storage cacheStorage = GetCacheStorage();
            bool cached = cacheStorage?.Exists(chartPath) ?? false;
            string usableFormat = cached ? DefaultAdapter.ChartFormatName : Format;

            // run it before anything else to avoid unnecessary variables
            if (!ChartPipeline.TryGetAdapter(usableFormat, out IChartAdapter chartAdapter) || chartAdapter == null)
            {
                pack.Logger.Add($"Missing {usableFormat} adapter, did it get registered?", LogLevel.Error);
                return null;
            }

            pack.Logger.Add($"can use cache? {cacheStorage != null}, {(cacheStorage != null ? $"was cached? {cached} targetting {usableFormat}" : "")}");
            using Stream chartStream = cached ? cacheStorage.GetStream(chartPath) : resources.GetStream(chartPath);
            if (chartStream == null)
            {
                pack.Logger.Add(
                    $"Failed to retrieve {songId} with difficulty {difficulty}. Please check the log file for more information",
                    LogLevel.Error);
                pack.Logger.Add($"Was looking for path {chartPath} but returned a null stream", outputToListeners: false);
                return null;
            }

            object prepChart = chartAdapter.Prepare(chartStream);
            if (prepChart is null)
            {
                pack.Logger.Add("Failed to prepare the chart stream into a usable object for adaptation.", LogLevel.Error);
                return null;
            }

            ChartData chartData = chartAdapter.Adapt(prepChart);
            if (chartData == null)
            {
                pack.Logger.Add("Adaptation failed, check the log file for more information.", LogLevel.Error);
                pack.Logger.Add($"There was an error while adapting {chartPath}.", outputToListeners: false);
                return null;
            }

            // pipe it down!
            if (cacheStorage != null && !cached)
            {
                Dictionary<string, object> tomlData = [];
                tomlData["metadata"] = chartData.Metadata;
                tomlData["beat_highlights"] = chartData.BeatHighlights;
                tomlData["timing_points"] = chartData.TimingPoints;
                tomlData["scroll_velocities"] = chartData.ScrollVelocities;
                tomlData["notes"] = chartData.Notes;

                string tomlString = Toml.FromModel(tomlData);
                // saving it with the same name is evil... breaks the file type but uhh it doesnt matter as long as its readable right?
                using Stream cacheStream = cacheStorage.GetStream(chartPath, FileAccess.Write);
                using StreamWriter writer = new StreamWriter(cacheStream);
                writer.Write(tomlString);
                writer.Flush();
            }

            return chartData;
        }

        // sanco here: i thought about exposing a CACHE folder inside the resource pack folder in case of being external
        // but exposing a variable to the storage cache could be potentially unsafe for storing unwanted files?
        [CanBeNull] private Storage cache;
        [CanBeNull] public Storage GetCacheStorage()
        {
            // passed storage is the host user storage folder scoped into the resource packs and resource pack id
            // in reality we should only pass down the cache storage to keep everything in a single place
            // but for the sake of making the resource pack the source of everything then we will do that
            return cache ??= storage?.GetStorageForDirectory("cache");
        }

        private string getAliasedPath(string songId)
        {
            // check for aliases in this path then join the target file
            string aliasPath = pack.GetPath($"Songs/{songId}");
            if (aliasPath.EndsWith('/')) // old me did this and uh its supposed to sanitize the path in case of adding a leading /
                aliasPath = aliasPath.Substring(0, aliasPath.Length - 1);

            return aliasPath;
        }
    }
}
