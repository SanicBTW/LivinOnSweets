using JetBrains.Annotations;

namespace LivinOnSweets.API.Song.Metadata
{
    [UsedImplicitly]
    public class SelectTable
    {
        [CanBeNull] public string Title { get; set; }

        [CanBeNull] public string Writer { get; set; }
    }
}
