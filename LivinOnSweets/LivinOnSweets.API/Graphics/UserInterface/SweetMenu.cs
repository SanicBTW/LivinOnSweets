using LivinOnSweets.API.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/UserInterface/OsuMenu.cs
    public partial class SweetMenu : Menu
    {
        protected const double DELAY_BEFORE_FADE_OUT = 50;
        protected const double FADE_DURATION = 280;

        protected bool WasOpened { get; private set; }

        protected SweetMenu(Direction direction, bool topLevelMenu = false) : base(direction, topLevelMenu)
        {
            BackgroundColour = Colour4.Black.Opacity(0.5f);

            MaskingContainer.CornerRadius = DrawableSweetMenuItem.CORNER_RADIUS;
            ItemsContainer.Padding = new MarginPadding(DrawableSweetMenuItem.CORNER_RADIUS);
        }

        protected override void AnimateOpen()
        {
            WasOpened = true;
            this.FadeIn(FADE_DURATION, Easing.OutQuint);
        }

        protected override void AnimateClose()
        {
            this.Delay(DELAY_BEFORE_FADE_OUT)
                .FadeOut(FADE_DURATION, Easing.OutQuint);

            WasOpened = false;
        }

        protected override void UpdateSize(Vector2 newSize)
        {
            if (Direction == Direction.Vertical)
            {
                Width = newSize.X;

                if (newSize.Y > 0)
                    this.ResizeHeightTo(newSize.Y, 300, Easing.OutQuint);
                else
                    // Delay until the fade out finishes from AnimateClose.
                    this.Delay(DELAY_BEFORE_FADE_OUT + FADE_DURATION).ResizeHeightTo(0);
            }
            else
            {
                Height = newSize.Y;
                if (newSize.X > 0)
                    this.ResizeWidthTo(newSize.X, 300, Easing.OutQuint);
                else
                    // Delay until the fade out finishes from AnimateClose.
                    this.Delay(DELAY_BEFORE_FADE_OUT + FADE_DURATION).ResizeWidthTo(0);
            }
        }

        protected override Menu CreateSubMenu() => new SweetMenu(Direction.Vertical)
        {
            Anchor = Direction == Direction.Horizontal ? Anchor.BottomLeft : Anchor.TopRight
        };

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableSweetMenuItem(item);

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) =>
            new SweetScrollContainer(direction) { ScrollbarOverlapsContent = false }; // its not really doing anything, weird
    }
}
