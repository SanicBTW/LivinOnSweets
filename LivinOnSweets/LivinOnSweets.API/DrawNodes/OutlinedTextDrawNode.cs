using LivinOnSweets.API.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace LivinOnSweets.API.DrawNodes
{
    // Replaces the shadow effect with a custom outline made from the same shadow arguments
    public partial class OutlinedTextDrawNode : SpriteTextDrawNode
    {
        protected new OutlinedSpriteText Source => (OutlinedSpriteText)base.Source;

        // Draws the text 8 times in all directions, using these offsets to move the draw quad
        private static readonly Vector2[] offsets =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1,  0),             new(1,  0),
            new(-1,  1), new(0,  1), new(1,  1)
        };

        protected float OutlineSize { get; set; } = 1F;

        public OutlinedTextDrawNode(SpriteText source) : base(source) { }

        public override void ApplyState()
        {
            base.ApplyState();

            if (Shadow)
            {
                ShadowOffset = Vector2.Zero;

                OutlineSize = Source.OutlineSize > 0 ? Source.OutlineSize : 1f;
            }
        }

        protected override void DrawShadow(IRenderer renderer, Quad shadowQuad, Texture texture, ColourInfo colour, Vector2 inflationPerct)
        {
            inflationPerct /= OutlineSize;

            foreach (Vector2 offset in offsets)
            {
                Quad offsetQuad = new Quad(
                    shadowQuad.TopLeft + (offset * OutlineSize),
                    shadowQuad.TopRight + (offset * OutlineSize),
                    shadowQuad.BottomLeft + (offset * OutlineSize),
                    shadowQuad.BottomRight + (offset * OutlineSize)
                    );

                base.DrawShadow(renderer, offsetQuad, texture, colour, inflationPerct);
            }
        }
    }
}
