using JetBrains.Annotations;
using LivinOnSweets.API.Song.Metadata;

namespace LivinOnSweets.API.Song
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SongMetadata
    {
        public string Id { get; set; }

        [CanBeNull] public StoryTable Story { get; set; }

        [CanBeNull] public SelectTable Select { get; set; }

        [CanBeNull] public AlbumTable Album { get; set; }

        [CanBeNull] public PreviewTable Preview { get; set; }

        [CanBeNull] public ExperienceTable Experience { get; set; }

        // Have to think about how to dynamically import the charts for other diffs
        // For now I'm gonna keep it like this
        public Dictionary<string, string> Charts { get; set; } = [];
    }
}
