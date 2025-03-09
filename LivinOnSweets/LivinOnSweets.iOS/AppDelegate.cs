using LivinOnSweets.Game;
using osu.Framework.iOS;

namespace LivinOnSweets.iOS
{
    /// <inheritdoc />
    public class AppDelegate : GameApplicationDelegate
    {
        /// <inheritdoc />
        protected override osu.Framework.Game CreateGame() => new LivinOnSweetsGame();
    }
}
