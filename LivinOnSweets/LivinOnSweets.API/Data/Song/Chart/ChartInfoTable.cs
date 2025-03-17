using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song.Chart
{
    public class ChartInfoTable
    {
        [CanBeNull] public string Difficulty { get; set; }

        // Feeling like I should use int bpms instead of float ones but whatever
        public float? Bpm { get; set; }

        public int? GridsPerBeat { get; set; }

        public float? NoteOffset { get; set; }

        [CanBeNull] public string GeneratedBy { get; set; } = "LivinOnSweets - ChartConverter v1";
    }
}
