using LivinOnSweets.API.Graphics.Sprites.DrawNodes;
using LivinOnSweets.API.Graphics.TextFx;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace LivinOnSweets.API.Graphics.Sprites
{
    // Inherits features from https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/DrawNodes/OutlinedTextDrawNode.cs
    // with improvements and such
    public partial class SweetSpriteText : SpriteText
    {
        private bool outline;

        /// <summary>
        /// True if an outline should be displayed around the text.
        /// </summary>
        public bool Outline
        {
            get => outline;
            set
            {
                if (outline == value)
                    return;

                outline = value;

                Invalidate(Invalidation.DrawNode);
            }
        }

        private Color4 outlineColour = new(0, 0, 0, 0.2f);

        /// <summary>
        /// The colour of the outline displayed around the text. An outline will only be displayed if the <see cref="Outline"/> property is set to true.
        /// </summary>
        public Color4 OutlineColour
        {
            get => outlineColour;
            set
            {
                if (outlineColour == value)
                    return;

                outlineColour = value;

                Invalidate(Invalidation.DrawNode);
            }
        }

        private float outlineSize = 1F;

        /// <summary>
        /// The size of the outline displayed around the text. An outline will only be displayed if the <see cref="Outline"/> property is set to true.
        /// </summary>
        public float OutlineSize
        {
            get => outlineSize;
            set
            {
                if (Precision.AlmostEquals(outlineSize, value))
                    return;

                outlineSize = value;

                // Invalidate the draw node to apply the new size
                Invalidate(Invalidation.DrawNode);
            }
        }

        protected override DrawNode CreateDrawNode() => new SweetSpriteTextDrawNode(this);

        public class SweetSpriteTextDrawNode(SweetSpriteText source) : SpriteTextDrawNode(source)
        {
            protected new SweetSpriteText Source => (SweetSpriteText)base.Source;

            public bool Outline { get; private set; }
            protected OutlineEffect OutlineEffect;

            public override void ApplyState()
            {
                base.ApplyState();

                Outline = Source.Outline;
                if (!Outline)
                {
                    if (OutlineEffect != null)
                        Effects.Remove(OutlineEffect);

                    return;
                }

                UpdateOutline();
            }

            protected virtual void UpdateOutline()
            {
                ColourInfo outlineColour = Source.OutlineColour;
                float outlineSize = Source.OutlineSize;

                if (OutlineEffect == null)
                    Effects.Add(OutlineEffect = new OutlineEffect(outlineColour, outlineSize));
                else
                    ((ITextEffect)OutlineEffect).Update(outlineColour, outlineSize);
            }
        }
    }
}
