using LivinOnSweets.API.DrawNodes;
using osu.Framework.Graphics;
using osu.Framework.Logging;

namespace LivinOnSweets.API.Sprites
{
    // TODO: Bind to the config speed
    public partial class SpeedChangeButton : SpritesheetButton
    {
        // If we subtractin speed then must look left right?
        protected override DrawNode CreateDrawNode() => new FlipDrawNode(this, double.IsNegative(Addition), false);

        private const string image_path = "MainMenu/UI/Select/SpeedSel.png";

        public readonly double Addition;

        public SpeedChangeButton(double addition = 0.5D) : base(image_path)
        {
            Addition = addition;
        }

        protected override void FireEvent()
        {
            Logger.Log($"Decrease speed gang by {Addition}");
        }
    }
}
