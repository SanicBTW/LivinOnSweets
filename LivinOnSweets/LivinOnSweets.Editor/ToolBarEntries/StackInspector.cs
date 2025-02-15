using System.Reflection;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Enum;
using LivinOnSweets.Editor.Containers;
using LivinOnSweets.Editor.Enum;
using LivinOnSweets.Editor.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osuTK;

namespace LivinOnSweets.Editor.ToolBarEntries;

// Screen previews will be added eventually, I just need to figure out a way to not kill the gpu with buffered containers :sigh:
internal partial class StackInspector() : ToolBarButton(FontAwesome.Solid.Clone, ToolBarActionType.TOGGLEABLE)
{
    private static FieldInfo stackFieldInfo = typeof(ScreenStack).GetField("stack", BindingFlags.NonPublic | BindingFlags.Instance);

    [Resolved] private DebugContainer debugContainer { get; set; }

    [Resolved] private EditorContainer editorView { get; set; }

    [Resolved] private GameStateManager stateManager { get; set; }

    private EditorWindow stackWindow;

    private ScreenStack gameScreenStack; // This targets the screen stack found on SGameScreen, not the one in debug container

    [BackgroundDependencyLoader]
    private void load()
    {
        EditorWindowOption refreshOption = new("refresh list", refreshList);
        LoadComponentAsync(stackWindow = new EditorWindow("stack inspector", refreshOption), editorView.Add);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        stackWindow.ScrollContent.Spacing = new Vector2(0, 6);
    }

    protected override void Toggled(bool newState)
    {
        stackWindow.ToggleVisibility();
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        // Search for the game screen stack that is not the master stack
        // We do this after the children since it can be really expensive
        if (gameScreenStack == null)
        {
            ScreenStack lastScreen = debugContainer.ChildrenOfType<ScreenStack>().Last();
            if (lastScreen == debugContainer.MasterStack)
                return;

            // I'm pretty sure there is only gonna be 2 screen stacks on the whole game
            // One (index 0) for the startup
            // Two (index 1) for the ACTUAL game
            // The reason why we run ChildrenOfType here is to save up a reference to avoid running it all the time
            gameScreenStack = lastScreen;
        }
    }

    private void refreshList()
    {
        stackWindow.ScrollContent.Clear();

        RuntimeState rtState = stateManager.RtState.Value;
        ScreenStack targetStack = rtState switch
        {
            RuntimeState.IN_GAME => gameScreenStack,
            _ => debugContainer.MasterStack
        };

        // aight bro
        Stack<IScreen> screensRefl = (Stack<IScreen>)stackFieldInfo.GetValue(targetStack)!;
        IScreen[] screens = screensRefl.ToArray();

        for (int i = 0; i < screens.Length; i++)
        {
            SweetScreen screen = (SweetScreen)screens[i];
            stackWindow.ScrollContent.Add(new StackCard(screen, i));
        }
    }

    // Will move this to another file + its still missing some functionality
    private partial class StackCard : Container
    {
        public const float CARD_MARGIN = 6;
        public const float CARD_WIDTH = EditorWindow.WIDTH - ((EditorWindow.INNER_STROKE + CARD_MARGIN) * 2);
        public const float CARD_HEIGHT = 82;

        public const float PREVIEW_MARGIN = 8;
        public const float PREVIEW_WIDTH = 118;
        public const float PREVIEW_HEIGHT = CARD_HEIGHT - (PREVIEW_MARGIN * 2);

        public static FontUsage DetailsFont = new(family: "GyeonggiTitle", size: 14F);

        private Container previewContainer;
        private SpriteText isLoaded;

        internal SweetScreen Screen;
        internal int StackPosition;

        public StackCard(SweetScreen targetScreen, int posInStack)
        {
            Screen = targetScreen;
            StackPosition = posInStack;

            Masking = true;
            CornerRadius = 10;
            Width = CARD_WIDTH - (CARD_MARGIN * 1.75F);
            Height = CARD_HEIGHT;

            Margin = new MarginPadding() { Left = CARD_MARGIN, Top = CARD_MARGIN };

            InternalChildren =
            [
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = EditorWindow.BackgroundColor
                },
                previewContainer = new Container()
                {
                    Name = "preview container",
                    Size = new Vector2(PREVIEW_WIDTH, PREVIEW_HEIGHT),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding() { Left = PREVIEW_MARGIN, Top = PREVIEW_MARGIN, Bottom = PREVIEW_MARGIN },
                    Masking = true,
                    CornerRadius = 10,
                    Child = getDefaultPreview()
                },
                new FillFlowContainer<SpriteText>()
                {
                    Name = "details container",
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    X = PREVIEW_WIDTH + PREVIEW_MARGIN,
                    Margin = new MarginPadding() { Left = PREVIEW_MARGIN, Top = PREVIEW_MARGIN * 1.5F, },
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(0, 2),
                    Children =
                    [
                        new SpriteText()
                        {
                            Text = getScreenName(),
                            Font = DetailsFont,
                        },
                        new SpriteText()
                        {
                            Text = $"depth: {posInStack}",
                            Font = DetailsFont.With(size: 10F)
                        },
                        isLoaded = new SpriteText()
                        {
                            Text = $"loaded: {Screen.IsAlive}",
                            Font = DetailsFont.With(size: 10F)
                        }
                    ]
                },
                new FillFlowContainer<SpriteIconButton>()
                {
                    Name = "toolbar container",
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding() { Right = PREVIEW_MARGIN / 2, Bottom = PREVIEW_MARGIN / 2 },
                    AutoSizeAxes = Axes.X,
                    Height = 24,
                    Children =
                    [
                        new RefreshScreenButton(this),
                    ]
                }
            ];
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // Should make it a bindable or something rather than changing the text everytime I guess
            isLoaded.Text = $"loaded: {Screen.IsAlive}";
        }

        private Container getDefaultPreview()
        {
            Container previewContent = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("#3a3a3a")
                    },
                    new SpriteIcon()
                    {
                        Icon = FontAwesome.Solid.Question,
                        Size = new Vector2(24),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                ]
            };

            return previewContent;
        }

        private string getScreenName()
        {
            return Screen.GetType().Name;
        }

        // TODO: Fix this
        private partial class RefreshScreenButton : SpriteIconButton
        {
            public const float BOUNCE_ROTATION = 25;

            private SweetScreen screen;
            private int stackPosition;
            private bool spinning;

            public RefreshScreenButton(StackCard card) : base(FontAwesome.Solid.RedoAlt)
            {
                screen = card.Screen;
                stackPosition = card.StackPosition;
                Action = refreshScreen;
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (spinning)
                    return false;

                Icon.RotateTo(-(BOUNCE_ROTATION * 0.75F), 250D, Easing.OutBack).Then()
                    .RotateTo(BOUNCE_ROTATION, 500D, Easing.OutBack);

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (spinning)
                    return;

                Icon.RotateTo(-BOUNCE_ROTATION, 250D, Easing.OutBack).Then()
                    .RotateTo(BOUNCE_ROTATION * 0.25F, 300D, Easing.OutBack).Then()
                    .RotateTo(0, 500D, Easing.OutBack);
            }

            private void refreshScreen()
            {
                // The screen parent can be null if the instance was disposed but the list wasn't updated
                // TODO: Automatically update the list once a screen gets refreshed
                if (screen.Parent == null)
                {
                    // Make it kickback, meaning that something bad happened while refreshing? or that the refresh couldn't be completed properly
                    OnHover(null);
                    return;
                }

                ScreenStack sStack = (ScreenStack)screen.Parent;

                // Show the user a confirm modal to fully refresh the stack, since the first (last screen) is the beginning of the stack
                if (!screen.ValidForResume && stackPosition != 0)
                {
                    OnHover(null);
                    return;
                }

                // Wanting to refresh any screen that isn't the first one would need to change the stack internally, which I guess it should be
                // by using reflection and creating a new stack with the existing screens while only replacing the target one
                if (stackPosition != 0)
                {
                    OnHover(null);
                    return;
                }

                Type targetType = screen.GetType();

                // We cannot figure out the necessary values to make the constructor, sooo avoid em
                bool hasCtorArgs = targetType.GetConstructors().Any(ctorinf => ctorinf.GetParameters().Length > 0);
                if (hasCtorArgs)
                {
                    OnHover(null);
                    return;
                }

                // Exit the current screen (0 lmao)
                sStack.Exit();

                // Push the new instance
                sStack.Push((IScreen)Activator.CreateInstance(targetType));

                spinning = true;
                Icon.Spin(500D, RotationDirection.Clockwise, Icon.Rotation, 1).OnComplete(_ => spinning = false);
            }
        }
    }
}
