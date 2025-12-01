using JetBrains.Annotations;

namespace LivinOnSweets.API.Song.Metadata
{
    [UsedImplicitly]
    public class PreviewTable
    {
        [CanBeNull] public string Audio { get; set; }
        [CanBeNull] public string Vocals { get; set; }
        public double Volume { get; set; }
    }
}
