using LivinOnSweets.API.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace LivinOnSweets.API.Graphics.UserInterface
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/UserInterface/DrawableOsuMenuItem.cs
    public partial class DrawableSweetMenuItem(MenuItem item) : Menu.DrawableMenuItem(item)
    {
        public const int CORNER_RADIUS = 5;
        public const int SPACING = 6;
        public const int TRANSITION_LENGTH = 80;
        public static Colour4 DefaultColor = Colour4.FromHex("3D5877");
        public static Colour4 DefaultTextColor = Colour4.FromHex("C9C9C9");
        public static FontUsage DefaultFont => new(family: "GyeonggiTitle", size: 14F);

        private Drawable cap;
        private ItemContainer item;

        protected override Drawable CreateContent() => item = new ItemContainer();

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = DefaultColor.MultiplyAlpha(0.7F);
            BackgroundColourHover = DefaultColor.MultiplyAlpha(0.58F);

            CornerRadius = CORNER_RADIUS;
            Masking = true;

            Margin = new MarginPadding() { Vertical = 4 };

            AddInternal(cap = createCap());
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // to display the content correctly
            item.Padding = new MarginPadding { Left = cap.DrawWidth + SPACING, Right = cap.DrawWidth + SPACING, Vertical = CORNER_RADIUS };
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            Alpha = IsActionable ? 1 : 0.2f;

            if (IsHovered && IsActionable)
            {
                item.BoldText.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);
                item.NormalText.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);
            }
            else
            {
                item.BoldText.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);
                item.NormalText.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);
            }
        }

        // since we cannot do custom rounding, we add a little sprite with the same color without rounding
        // at the end of the sprite with the same size as the rounding
        private static Container createCap(float width = 10)
        {
            Container roundCap = new Container
            {
                Width = width,
                RelativeSizeAxes = Axes.Y,
                CornerRadius = CORNER_RADIUS,
                Masking = true,
                Child = new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = DefaultColor
                }
            };

            Box roundHide = new Box
            {
                RelativeSizeAxes = Axes.Y,
                Width = CORNER_RADIUS,
                Colour = DefaultColor,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            };

            return new Container()
            {
                Width = width,
                RelativeSizeAxes = Axes.Y,
                Children =
                [
                    roundCap,
                    roundHide
                ]
            };
        }

        protected partial class ItemContainer : Container, IHasText
        {

            public LocalisableString Text
            {
                get => NormalText.Text;
                set
                {
                    NormalText.Text = value;
                    BoldText.Text = value;
                }
            }

            public readonly SpriteText NormalText;
            public readonly SpriteText BoldText;

            public ItemContainer()
            {
                AutoSizeAxes = Axes.Both;
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(SPACING),
                    Direction = FillDirection.Horizontal,
                    Children =
                    [
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Children =
                            [
                                NormalText = new SweetSpriteText
                                {
                                    AlwaysPresent = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Colour = DefaultTextColor,
                                    Font = DefaultFont,
                                },
                                BoldText = new SweetSpriteText
                                {
                                    AlwaysPresent = true,
                                    Alpha = 0,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Colour = DefaultTextColor,
                                    Font = DefaultFont.With(weight: "Bold")
                                }
                            ]
                        }
                    ]
                };
            }
        }
    }
}
