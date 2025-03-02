using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song
{
    public class SelectTable
    {
        [CanBeNull] public string Title { get; set; }

        [CanBeNull] public string Writer { get; set; }
    }
}
