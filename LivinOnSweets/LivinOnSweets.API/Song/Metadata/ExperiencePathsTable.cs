using JetBrains.Annotations;

namespace LivinOnSweets.API.Song.Metadata
{
    [UsedImplicitly]
    public class ExperiencePathsTable
    {
        [CanBeNull] public string Vocals { get; set; }

        [CanBeNull] public string Amv { get; set; }
    }
}
