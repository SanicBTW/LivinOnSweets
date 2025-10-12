using JetBrains.Annotations;

namespace LivinOnSweets.API.Song.Metadata
{
    [UsedImplicitly]
    public class StoryTable
    {
        public bool Locked { get; set; }

        public string Chapter { get; set; }

        public List<string> Unlocks { get; set; } = [];
    }
}
