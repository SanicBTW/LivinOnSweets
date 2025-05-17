using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using LivinOnSweets.API.Graphics.TextFx;
using LivinOnSweets.API.Rendering;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Text;
using osuTK;

namespace LivinOnSweets.API.Graphics.Sprites.DrawNodes
{
    // A copy of SpriteText_DrawNode which uses Reflection to access some private fields on SpriteText
    // Now includes a custom implementation of effects which modify the rendering of the letter, making the rendering modular
    // https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/DrawNodes/SpriteTextDrawNode.cs
    public partial class SpriteTextDrawNode(SpriteText source) : TexturedShaderDrawNode(source)
    {
        private static MethodInfo premultShadowOffsetGetField =>
            typeof(SpriteText).GetProperty("premultipliedShadowOffset", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        private static MethodInfo textCharsList =>
            typeof(SpriteText).GetProperty("characters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod;

        protected new SpriteText Source => (SpriteText)base.Source;

        public bool Shadow { get; private set; }
        protected ShadowEffect ShadowEffect;

        [CanBeNull] private List<ScreenSpaceCharacterPart> parts;

        protected List<ITextEffect> Effects = [];

        public override void ApplyState()
        {
            base.ApplyState();

            UpdateScreenSpaceCharacters();

            Shadow = Source.Shadow;
            if (!Shadow)
            {
                if (ShadowEffect != null)
                    Effects.Remove(ShadowEffect);

                return;
            }

            UpdateShadow();
        }

        protected override void Draw(IRenderer renderer)
        {
            Debug.Assert(parts != null);

            base.Draw(renderer);

            BindTextureShader(renderer);

            foreach (ITextEffect effect in Effects)
                effect.Recalculate(DrawColourInfo.Colour);

            for (int i = 0; i < parts.Count; i++)
            {
                ScreenSpaceCharacterPart part = parts[i];
                foreach (var effect in Effects.Where(effect => effect.Phase == DrawingPhase.PreDraw))
                {
                    effect.Apply(renderer, part, DrawColourInfo.Colour);
                }

                renderer.DrawQuad(parts[i].Texture, parts[i].DrawQuad, DrawColourInfo.Colour, inflationPercentage: parts[i].InflationPercentage);

                foreach (var effect in Effects.Where(effect => effect.Phase == DrawingPhase.PostDraw))
                {
                    effect.Apply(renderer, part, DrawColourInfo.Colour);
                }
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

        protected virtual void UpdateShadow()
        {
            ColourInfo shadowColour = Source.ShadowColour;
            Vector2 shadowOffset = (Vector2)premultShadowOffsetGetField.Invoke(Source, [])!;

            if (ShadowEffect == null)
                Effects.Add(ShadowEffect = new ShadowEffect(shadowColour, shadowOffset));
            else
                ((ITextEffect)ShadowEffect).Update(shadowColour, shadowOffset);
        }
    }
}
