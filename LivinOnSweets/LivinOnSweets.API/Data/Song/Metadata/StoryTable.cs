using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song.Metadata
{
    public class StoryTable
    {
        public bool Locked { get; set; }

        [CanBeNull] public string Chapter { get; set; }

        [CanBeNull]
        public List<string> Unlocks { get; set; } = [];
    }
}
