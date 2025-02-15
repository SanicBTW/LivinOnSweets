using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace LivinOnSweets.Editor.Sprites;

internal partial class SpriteIconButton : Button
{
    protected SpriteIcon Icon;

    public SpriteIconButton(IconUsage btnIcon)
    {
        // No autosizing since it breaks all the layout with the animations lmao
        Size = new Vector2(24);
        Padding = new MarginPadding(4);
        InternalChild = Icon = new SpriteIcon()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Icon = btnIcon,
            Size = new Vector2(16),
        };
    }
}
