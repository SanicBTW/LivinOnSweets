using JetBrains.Annotations;

namespace LivinOnSweets.API.Data.Song
{
    public class PreviewTable
    {
        [CanBeNull] public string Audio { get; set; }

        public double Volume { get; set; }
    }
}
