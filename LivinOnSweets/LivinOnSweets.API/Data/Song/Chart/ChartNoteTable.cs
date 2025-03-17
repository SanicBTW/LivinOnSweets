using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song.Chart
{
    public class ChartNoteTable
    {
        [CanBeNull] public string Type { get; set; }

        public int Lane { get; set; }

        [CanBeNull] public NoteBeatGridTable Start { get; set; }

        // For long notes
        [CanBeNull] public NoteBeatGridTable End { get; set; }

        // Will refer to the metadata gpb if this is null
        public int? GridsPerBeat { get; set; }

        public bool Fever { get; set; }
    }
}
