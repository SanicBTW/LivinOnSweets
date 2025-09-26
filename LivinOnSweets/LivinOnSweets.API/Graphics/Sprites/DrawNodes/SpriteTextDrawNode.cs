using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using LivinOnSweets.API.Graphics.TextFx;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Text;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites.DrawNodes
{
    // A copy of SpriteText_DrawNode which uses Reflection to access some private fields on SpriteText
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/DrawNodes/SpriteTextDrawNode.cs
    public partial class SpriteTextDrawNode(SpriteText source) : TexturedShaderDrawNode(source)
    {
        private static MethodInfo premultShadowOffsetGetField =>
            typeof(SpriteText).GetProperty("premultipliedShadowOffset", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        private static MethodInfo textCharsList =>
            typeof(SpriteText).GetProperty("characters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        protected new SpriteText Source => (SpriteText)base.Source;

        public bool Shadow { get; private set; }
        protected ITextEffect ShadowEffect;

        [CanBeNull] private List<ScreenSpaceCharacterPart> parts;

        public override void ApplyState()
        {
            base.ApplyState();

            UpdateScreenSpaceCharacters();

            Shadow = Source.Shadow;
            UpdateFx();
        }

        protected override void Draw(IRenderer renderer)
        {
            Debug.Assert(parts != null);

            base.Draw(renderer);

            BindTextureShader(renderer);

            RecalculateFx();

            for (int i = 0; i < parts.Count; i++)
            {
                ScreenSpaceCharacterPart part = parts[i];

                ApplyPreDrawFx(renderer, part);
                renderer.DrawQuad(parts[i].Texture, parts[i].DrawQuad, DrawColourInfo.Colour, inflationPercentage: parts[i].InflationPercentage);
                ApplyPostDrawFx(renderer, part);
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
                parts.Add(new ScreenSpaceCharacterPart
                {
                    DrawQuad = Source.ToScreenSpace(character.DrawRectangle.Inflate(inflationAmount)),
                    InflationPercentage = new Vector2(
                        character.DrawRectangle.Size.X == 0 ? 0 : inflationAmount.X / character.DrawRectangle.Size.X,
                        character.DrawRectangle.Size.Y == 0 ? 0 : inflationAmount.Y / character.DrawRectangle.Size.Y),
                    Texture = character.Texture
                });
            }
        }

        protected virtual void RecalculateFx()
        {
            if (Shadow)
                ShadowEffect.Recalculate(DrawColourInfo.Colour);
        }

        /// <summary>
        /// Gets called before drawing into the screen, used to modify the <paramref name="part"/> quad.
        /// </summary>
        /// <param name="renderer">The renderer used inside the Draw function.</param>
        /// <param name="part">The part which is currently getting drawn onto the screen.</param>
        protected virtual void ApplyPreDrawFx(IRenderer renderer, ScreenSpaceCharacterPart part)
        {
            if (Shadow)
                ShadowEffect.Apply(renderer, part, DrawColourInfo.Colour);
        }

        /// <summary>
        /// Gets called after drawing into the screen, useful in case you want to analyze the <paramref name="part"/> or modify the output quad.
        /// </summary>
        /// <param name="renderer">The renderer used inside the Draw function.</param>
        /// <param name="part">The part which is currently getting drawn onto the screen.</param>
        protected virtual void ApplyPostDrawFx(IRenderer renderer, ScreenSpaceCharacterPart part) { }

        /// <summary>
        /// Updates all the effects based on the <see cref="SweetSpriteText"/> implementation.
        /// <remarks>The base implementation contains the <see cref="OutlineEffect"/>.</remarks>
        /// </summary>
        protected virtual void UpdateFx()
        {
            if (Shadow)
                updateShadow();
        }

        private void updateShadow()
        {
            ColourInfo shadowColour = Source.ShadowColour;
            Vector2 shadowOffset = (Vector2)premultShadowOffsetGetField.Invoke(Source, [])!;

            if (ShadowEffect == null)
                ShadowEffect = new ShadowEffect(shadowColour, shadowOffset);
            else
                ShadowEffect.Update(shadowColour, shadowOffset);
        }
    }
}
