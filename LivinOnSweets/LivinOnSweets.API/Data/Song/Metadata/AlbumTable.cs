using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song.Metadata
{
    public class AlbumTable
    {
        [CanBeNull] public string Cover { get; set; }

        [CanBeNull] public string Lock { get; set; }

        [CanBeNull] public string Disc { get; set; }
    }
}
