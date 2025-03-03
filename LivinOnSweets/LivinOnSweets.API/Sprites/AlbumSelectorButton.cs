using LivinOnSweets.API.DrawNodes;
using osu.Framework.Graphics;

namespace LivinOnSweets.API.Sprites
{
    // Basic enough lol, will fire an action to change the CurSelected variable in SongSelectionScreen
    // Like speed selector button
    public partial class AlbumSelectorButton : SpritesheetButton
    {
        protected override DrawNode CreateDrawNode() => new FlipDrawNode(this, false, int.IsNegative(addition));

        private const string image_path = "MainMenu/UI/Select/SongSel.png";

        public Action<int> Action;

        private readonly int addition;

        public AlbumSelectorButton(int addition = 1) : base(image_path)
        {
            this.addition = addition;
        }

        protected override void FireEvent() => Action?.Invoke(addition);
    }

}
