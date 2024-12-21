using LivinOnSweets.Game;
using osu.Framework;
using osu.Framework.Platform;

namespace LivinOnSweets.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"LivinOnSweets", new() { FriendlyGameName = "LIVIN' ON SWEETS!" }))
            using (osu.Framework.Game game = new LivinOnSweetsGame())
                host.Run(game);
        }
    }
}
