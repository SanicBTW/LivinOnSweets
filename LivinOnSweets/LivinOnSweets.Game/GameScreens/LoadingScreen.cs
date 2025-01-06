using LivinOnSweets.API.Container;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.Game.GameScreens
{
    public partial class LoadingScreen : SweetScreen
    {
        [BackgroundDependencyLoader]
        private void load(MainMenuStore mmStore)
        {
            InternalChildren = new Drawable[]
            {
                // This is made to properly position Airi with the background
                // Since the container "resizes", Airi is anchored to the Centre Right of her hitbox, which anchors to the
                // right part of the screen and when resized it wouldnt look correctly
                new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Sprite()
                        {
                            Texture = mmStore.Get("MainMenu/Backgrounds/LoadScreen.png"),
                            /*RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,*/
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new AiriVibe()
                    }
                }
            };
        }
    }
}
