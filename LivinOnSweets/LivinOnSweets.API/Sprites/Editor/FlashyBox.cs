// https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/FlashyBox.cs

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;

namespace LivinOnSweets.API.Sprites.Editor
{
    internal partial class FlashyBox : Box
    {
        private Drawable target;
        public Drawable Target
        {
            set => target = value;
        }

        private readonly Func<Drawable, Quad> getScreenSpaceQuad;

        public FlashyBox(Func<Drawable, Quad> getScreenSpaceQuad)
        {
            this.getScreenSpaceQuad = getScreenSpaceQuad;
        }

        public override Quad ScreenSpaceDrawQuad => target == null ? new Quad() : getScreenSpaceQuad(target);
    }
}
