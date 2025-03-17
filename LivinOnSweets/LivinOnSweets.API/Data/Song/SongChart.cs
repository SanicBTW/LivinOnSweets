using JetBrains.Annotations;
using LivinOnSweets.API.Data.Song.Chart;

namespace LivinOnSweets.API.Data.Song
{
    // Ported over from the ChartConverter, will probably make it a NuGet package at some point
    // Standard chart format for LCFv1
    public class SongChart
    {
        [CanBeNull] public ChartInfoTable Metadata { get; set; }

        [CanBeNull] public List<ChartBeatHighlight> BeatHighlights { get; set; } = [];

        [CanBeNull] public List<ChartNoteTable> Notes { get; set; } = [];
    }
}
