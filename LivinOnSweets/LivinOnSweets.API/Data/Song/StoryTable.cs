using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song
{
    public class StoryTable
    {
        public bool Locked { get; set; }

        [CanBeNull] public string Chapter { get; set; }

        [DataMember(Name = "unlocks")]
        [CanBeNull] public List<string> Unlocks { get; set; }

        public StoryTable()
        {
            Unlocks = [];
        }
    }
}
