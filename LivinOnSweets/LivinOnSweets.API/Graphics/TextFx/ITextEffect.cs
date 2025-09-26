using JetBrains.Annotations;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;

namespace LivinOnSweets.API.Graphics.TextFx
{
    public interface ITextEffect
    {
        // I should look into this more in-depth, default effects as in SpriteTextDrawNode get added to the default list
        // but maybe get updated twice and incorrectly, so the best choice here is to manually update specific effects

        /// <summary>
        /// Update internal effect variables, calling may differ between DrawNodes.
        /// </summary>
        /// <param name="updated">The updated parameters of the effect or object if any.</param>
        public void Update([ItemCanBeNull] params dynamic[] updated) { }

        /// <summary>
        /// Gets called each Draw call, useful if needed to calculate anything once per draw
        /// </summary>
        /// <param name="colour">The colour of the text.</param>
        public void Recalculate(ColourInfo colour) { }

        /// <summary>
        /// Applies an effect over the letter.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="part">The part to draw.</param>
        /// <param name="colour">The colour of the text.</param>
        public void Apply(IRenderer renderer, ScreenSpaceCharacterPart part, ColourInfo colour);
    }
}
