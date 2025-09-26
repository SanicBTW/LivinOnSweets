using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // https://github.com/ppy/osu/blob/e1ba1b45b0b7b32410ef4302044e71fe833edc0b/osu.Game/Graphics/UserInterface/OsuContextMenu.cs
    public partial class SweetContextMenu : SweetMenu
    {
        public SweetContextMenu() : base(Direction.Vertical)
        {
            MaskingContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.Black.Opacity(0.1f),
                Radius = 4,
            };

            MaxHeight = 250;
        }
    }
}
