using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace LivinOnSweets.API.Sprites.Editor
{
    public partial class EditorButton : BasicButton
    {
        public EditorButton()
        {
            AutoSizeAxes = Axes.X;
            Height = 48;

            Masking = true;
            CornerRadius = 6;
            DisabledColour = Colour4.White.Darken(0.15f);
        }

        public Colour4 TextColor
        {
            get => SpriteText.Colour;
            set => SpriteText.Colour = value;
        }

        protected override SpriteText CreateText() => new()
        {
            Depth = -1,
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            Font = new FontUsage(family: "DNFBitBit", size: 24F),
            Margin = new MarginPadding(8)
        };
    }
}
