using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;
using osuTK;
using osuTK.Graphics;

namespace LivinOnSweets.API.DrawNodes
{
    // A copy of SpriteText_DrawNode which uses Reflection to access some private fields on SpriteText
    public partial class SpriteTextDrawNode : TexturedShaderDrawNode
    {
        private static MethodInfo premultShadowOffsetGetField =>
            typeof(SpriteText).GetProperty("premultipliedShadowOffset", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        private static MethodInfo textCharsList =>
            typeof(SpriteText).GetProperty("characters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        protected new SpriteText Source => (SpriteText)base.Source;

        public bool Shadow { get; private set; }
        protected ColourInfo ShadowColour { get; private set; }
        protected Vector2 ShadowOffset { get; set; }

        [CanBeNull] private List<ScreenSpaceCharacterPart> parts;

        public SpriteTextDrawNode(SpriteText source) : base(source) { }

        public override void ApplyState()
        {
            base.ApplyState();

            UpdateScreenSpaceCharacters();
            Shadow = Source.Shadow;

            if (Shadow)
            {
                ShadowColour = Source.ShadowColour;
                ShadowOffset = (Vector2)premultShadowOffsetGetField.Invoke(Source, [])!;
            }
        }

        protected override void Draw(IRenderer renderer)
        {
            Debug.Assert(parts != null);

            base.Draw(renderer);

            BindTextureShader(renderer);

            Color4 avgColour = DrawColourInfo.Colour.AverageColour;
            float shadowAlpha = MathF.Pow(Math.Max(Math.Max(avgColour.R, avgColour.G), avgColour.B), 2);

            ColourInfo finalShadowColour = DrawColourInfo.Colour;
            finalShadowColour.ApplyChild(ShadowColour.MultiplyAlpha(shadowAlpha));

            for (int i = 0; i < parts.Count; i++)
            {
                if (Shadow)
                    DrawShadow(renderer, parts[i].DrawQuad, parts[i].Texture, finalShadowColour, parts[i].InflationPercentage);

                renderer.DrawQuad(parts[i].Texture, parts[i].DrawQuad, DrawColourInfo.Colour, inflationPercentage: parts[i].InflationPercentage);
            }

            UnbindTextureShader(renderer);
        }

        protected virtual void UpdateScreenSpaceCharacters()
        {
            List<TextBuilderGlyph> characters = (List<TextBuilderGlyph>)textCharsList.Invoke(Source, [])!;

            int partCount = characters.Count;
            if (parts == null)
                parts = new List<ScreenSpaceCharacterPart>(partCount);
            else
            {
                parts.Clear();
                parts.EnsureCapacity(partCount);
            }

            Vector2 inflationAmount = DrawInfo.MatrixInverse.ExtractScale().Xy;

            foreach (TextBuilderGlyph character in characters)
            {
                parts.Add(new ScreenSpaceCharacterPart()
                {
                    DrawQuad = Source.ToScreenSpace(character.DrawRectangle.Inflate(inflationAmount)),
                    InflationPercentage = new Vector2(
                        character.DrawRectangle.Size.X == 0 ? 0 : inflationAmount.X / character.DrawRectangle.Size.X,
                        character.DrawRectangle.Size.Y == 0 ? 0 : inflationAmount.Y / character.DrawRectangle.Size.Y),
                    Texture = character.Texture
                });
            }
        }

        protected virtual void DrawShadow(IRenderer renderer, Quad shadowQuad, Texture texture, ColourInfo colour, Vector2 inflationPerct)
        {
            renderer.DrawQuad(texture,
                new Quad(
                    shadowQuad.TopLeft + ShadowOffset,
                    shadowQuad.TopRight + ShadowOffset,
                    shadowQuad.BottomLeft + ShadowOffset,
                    shadowQuad.BottomRight + ShadowOffset),
                colour, inflationPercentage: inflationPerct);
        }

        protected struct ScreenSpaceCharacterPart
        {
            /// <summary>
            /// The screen-space quad for the character to be drawn in.
            /// </summary>
            public Quad DrawQuad;

            /// <summary>
            /// Extra padding for the character's texture.
            /// </summary>
            public Vector2 InflationPercentage;

            /// <summary>
            /// The texture to draw the character with.
            /// </summary>
            public Texture Texture;
        }
    }
}

