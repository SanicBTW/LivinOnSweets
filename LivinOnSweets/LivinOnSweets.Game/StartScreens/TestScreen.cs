using LivinOnSweets.API.Container;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;

namespace LivinOnSweets.Game.Screens
{
    public partial class TestScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [BackgroundDependencyLoader]
        private void load(LargeTextureStore largeTex)
        {
            Sprite spr;
            InternalChildren = new Drawable[]
            {
                spr = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = largeTex.Get("StoryMode/Characters/NatsuBand02.png"),
                    Scale = new Vector2(0.5f)
                }
            };

            spr.Spin(8000D, RotationDirection.Clockwise);
        }


        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {

        }
    }
}
