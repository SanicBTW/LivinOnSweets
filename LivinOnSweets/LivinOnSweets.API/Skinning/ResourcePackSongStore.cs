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
    public class ResourcePackSongStore(ResourcePack pack, Storage storage) : ISongStore
    {
        // since we cannot access the internal resource pack store we make one based on a possible route
        private readonly IResourceStore<byte[]> resources = pack.RetrievePackResources();

        // id / metadata, will be saved upon getmeta call
        private readonly Dictionary<string, SongMetadata> metaCache = [];

        public string Format => pack.PackInfo.Songs.Format;

        public IEnumerable<string> GetAvailableSongs() => pack.PackInfo.Songs.Available;

        public SongMetadata GetMetadata(string songId)
        {
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
            // run it before anything else to avoid unnecessary variables
            if (!ChartPipeline.TryGetAdapter(Format, out IChartAdapter chartAdapter) || chartAdapter == null)
            {
                pack.Logger.Add($"Missing {Format} adapter, did it get registered?", LogLevel.Error);
                return null;
            }

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

            using Stream chartStream = resources.GetStream(chartPath);
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

            return chartData;
        }

        // sanco here: i thought about exposing a CACHE folder inside the resource pack folder in case of being external
        // but exposing a variable to the storage cache could be potentially unsafe for storing unwanted files?
        private Storage cache;
        public Storage GetCacheStorage()
        {
            // passed storage is the host user storage folder scoped into the resource packs and resource pack id
            // in reality we should only pass down the cache storage to keep everything in a single place
            // but for the sake of making the resource pack the source of everything then we will do that
            return cache ??= storage.GetStorageForDirectory("cache");
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
