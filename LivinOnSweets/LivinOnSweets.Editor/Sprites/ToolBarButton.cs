using JetBrains.Annotations;
using LivinOnSweets.API.Containers;
using LivinOnSweets.Editor.Enum;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace LivinOnSweets.Editor.Sprites;

internal partial class ToolBarButton : AutoSizeOnceContainer
{
    public static Colour4 InactiveColor = Colour4.FromHex("#6C6C6C");
    public static Colour4 ActiveColor = Colour4.FromHex("#D9D9D9");

    private float bounceHeight => DrawHeight * 0.1f;

    private Action action;
    private ToolBarActionType actionBehaviour;

    private SpriteIcon icon;
    private Box indicator;

    public ToolBarButton(IconUsage toolIcon, ToolBarActionType actionType) : base(Axes.Y)
    {
        actionBehaviour = actionType;
        AutoSizeAxes = Axes.Both;

        AutoSizeOnceContainer<SpriteIcon> iconMask = new AutoSizeOnceContainer<SpriteIcon>(Axes.Y)
        {
            Name = "icon mask",
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Padding = new MarginPadding() { Bottom = 22f },
            Child = icon = new SpriteIcon()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = toolIcon,
                Size = new Vector2(34),
                Colour = ActiveColor
            },
        };

        Container padCont = new Container()
        {
            Name = "indicator padding container",
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            Padding = new MarginPadding() { Top = 22f },
            Child = new Container()
            {
                Name = "indicator mask",
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5f,
                CornerExponent = 10f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = indicator = new Box()
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 11,
                    Colour = InactiveColor,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            }
        };

        InternalChildren =
        [
            iconMask,
            padCont
        ];

        setupBehaviour();
    }

    // Animations

    // Mimic the Windows 11 taskbar animations
    private void jumpInAnimation()
    {
        icon.MoveToY(bounceHeight, 250D, Easing.OutBack).Then()
            .MoveToY(-(bounceHeight * 0.25f), 300D, Easing.OutBack).Then()
            .MoveToY(0, 500D, Easing.OutBack);
    }

    private void jumpOutAnimation()
    {
        icon.MoveToY(-bounceHeight, 250D, Easing.OutBack).Then()
            .MoveToY(bounceHeight * 0.25f, 300D, Easing.OutBack).Then()
            .MoveToY(0, 500D, Easing.OutBack);
    }

    // This doesn't really mimic the Windows 11 focus change animation but because it scales, it modifies the container soo we dont want that
    private void focusAnimation()
    {
        icon.MoveToY(-(bounceHeight * 0.3f), 250D, Easing.OutQuad).Then()
            .MoveToY(0, 500D, Easing.OutQuad);
    }

    // Action Behaviour
    private void setupBehaviour()
    {
        action = actionBehaviour switch
        {
            ToolBarActionType.TOGGLEABLE => actionToggle,
            _ => actionClick,
        };
    }

    private bool state;
    private void actionToggle()
    {
        state = !state;
        Colour4 newColor = state ? ActiveColor : InactiveColor;

        indicator.FadeColour(newColor, 200D);

        if (state)
            jumpInAnimation();
        else
            jumpOutAnimation();

        Toggled(state);
    }

    private void actionClick()
    {
        // Its not like a toggle, we need feedback in some way I guess
        indicator.FadeColour(ActiveColor, 200D).Then().FadeColour(InactiveColor, 200D);

        focusAnimation();

        Clicked();
    }

    // ClickableContainer implementation
    protected override bool OnClick(ClickEvent e)
    {
        action?.Invoke();
        return true;
    }

    // Custom callbacks
    protected virtual void Toggled(bool newState) { }

    protected virtual void Clicked() { }
}
