using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song
{
    public class SongMetadata
    {
        public string Id { get; set; }

        [CanBeNull] public StoryTable Story { get; set; }

        [CanBeNull] public SelectTable Select { get; set; }

        [CanBeNull] public AlbumTable Album { get; set; }

        [CanBeNull] public PreviewTable Preview { get; set; }

        // Have to think about how to dynamically import the charts for other diffs
        // For now I'm gonna keep it like this
        public Dictionary<string, string> Charts { get; set; }

        public SongMetadata()
        {
            Charts = new Dictionary<string, string>();
        }
    }
}
