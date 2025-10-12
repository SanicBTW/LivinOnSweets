using JetBrains.Annotations;

namespace LivinOnSweets.API.Song.Metadata
{
    [UsedImplicitly]
    public class ExperienceTable
    {
        public bool Vocals { get; set; }
        public bool Amv { get; set; }
        [CanBeNull] public ExperiencePathsTable Paths { get; set; }
    }
}
