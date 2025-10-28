using JetBrains.Annotations;
using LivinOnSweets.API.Enums;

namespace LivinOnSweets.API.SaveData
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SongStats
    {
        public int BestScore { get; set; }
        public Dictionary<JudgementResult, int> Judgements { get; set; } = [];
        public int MaxCombo { get; set; }
        public SongRating Rank { get; set; } =
            SongRating.None; // i honestly dont know hy im saving this if the original game doesnt, oh well
        public SongClearanceStat Clearance { get; set; } =
            SongClearanceStat.None; // should make it flags so i can mix them?
    }
}
