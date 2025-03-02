using LivinOnSweets.API.DrawNodes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Sprites
{
    public partial class OutlinedSpriteText : SpriteText
    {
        private float outlineSize = 1F;

        public float OutlineSize
        {
            get => outlineSize;
            set
            {
                outlineSize = value;

                // Invalidate the draw node to apply the new size
                Invalidate(Invalidation.DrawNode);
            }
        }

        protected override DrawNode CreateDrawNode() => new OutlinedTextDrawNode(this);
    }
}
