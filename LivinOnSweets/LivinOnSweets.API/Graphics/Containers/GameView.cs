using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.StateMachines;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace LivinOnSweets.API.Graphics.Containers
{
    // Port of the old code with changes to adapt to the new system
    /// <summary>
    /// Container which has a <see cref="SweetScreenStack"/> to act like an "embedded" frame for the game screens.
    /// </summary>
    public partial class GameView : CompositeDrawable
    {
        [Resolved] private GameStateManager stateManager { get; set; }
        [Resolved] private GameSession gameSession { get; set; }

        // Used to block the propagation of input events in the screen when unwanted
        private readonly BindableBool propagateInput = new();
        public override bool PropagateNonPositionalInputSubTree => propagateInput.Value;
        public override bool PropagatePositionalInputSubTree => propagateInput.Value;

        // The values provided by old me did in fact NOT look properly
        // buncho bindables to automate this thing
        public readonly Bindable<Vector2> ContainerSize = new(new Vector2(885, 498));
        public readonly BindableMarginPadding GameMargin = new();

        public readonly Bindable<Vector2> GameSize = new(new Vector2(1280, 720));
        public readonly Bindable<DrawSizePreservationStrategy> SizeStrategy = new();

        private Box background;
        private DrawSizePreservingFillContainer sizePreservingContainer;
        private SweetScreenStack screenStack;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;

            InternalChildren =
            [
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#3a3a3a"), // color of the game container,
                    Alpha = 0f,
                },
                // All of the content will be sized as 1280x720, to avoid issues with positioning and scaling artifacts
                // This could change in the future however, allowing the users to uncap the size of the screens
                // thats actually great content for an update lmao
                // These comments are from way back from February or so, I can confirm, the rewrite still DOESN'T have this implemented
                sizePreservingContainer = new DrawSizePreservingFillContainer
                {
                    // Masking container to cap the content of the screen stack
                    // the reason why we are not doing it on the parent container (this one on top)
                    // is because its relative size axes and scales the children to fit the target size, sooo
                    // we need a helping container to mask the shi properly in the desired size
                    Child = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Child = screenStack = createScreenStack(),
                    }
                }
            ];

            propagateInput.BindTo(gameSession.InputEnabled);

            GameSize.BindValueChanged((ev) =>
            {
                sizePreservingContainer.TargetDrawSize = ev.NewValue;
                sizePreservingContainer.Child.Size = ev.NewValue; // will target our masking container
            }, true);
            SizeStrategy.BindValueChanged((ev) => sizePreservingContainer.Strategy = ev.NewValue, true);

            ContainerSize.BindValueChanged((ev) => Size = ev.NewValue, true);
            GameMargin.BindValueChanged((ev) => Margin = ev.NewValue, true);
        }

        // could make this container a resource pack reloadable drawable but nah
        public void UpdateLayout(int gameUpdate)
        {
            bool isAntiqueSeraphim = gameUpdate >= (int)GameUpdateVersion.AntiqueSeraphim;

            // this modifies the default value, in case of backing out within the same resource pack
            GameMargin.Default = new MarginPadding
            {
                Left = isAntiqueSeraphim ? 57 : 94,
                Bottom = isAntiqueSeraphim ? 13 : 0,
                Top = isAntiqueSeraphim ? 0 : 108
            };

            // in case of modifying the size too, modify the default value so it persists if the bindable value changes
            ContainerSize.SetDefault();
            GameMargin.SetDefault();
        }

        public void PushScreen(GameScreenData screenData)
        {
            switch (stateManager.GameplayMachine.CurrentState.Value)
            {
                case GameplayState.NotReady:
                    // i disabled most of the animations on first load since its blocked by the transition background on startup screen & sgame screen
                    SweetScreen nextScreen = screenData.CreateScreen<SweetScreen>();
                    if (nextScreen == null)
                    {
                        enableBacking();
                        // the runtime machine backing is called on this function since this shouldnt modify the runtime at all but only the gameplay
                        screenData.OnError?.Invoke();
                        return;
                    }

                    background.FadeIn(1000D, Easing.OutQuint).OnComplete(_ =>
                    {
                        LoadComponentAsync(nextScreen, _ =>
                        {
                            screenStack.Push(nextScreen);
                            screenStack.FadeTo(1, 250D, Easing.OutQuint).Delay(150).ScaleTo(1, 500D, Easing.OutQuart);

                            enableBacking();
                            stateManager.GameplayMachine.UpdateState(false);
                            screenData.OnLoad?.Invoke();
                        });
                    });
                    break;

                // Only load up the next screen WHEN uninitialized, otherwise just invoke the load callback which does the magic
                default:
                    screenData.OnLoad?.Invoke();
                    ScheduleAfterChildren(enableBacking);
                    break;
            }
        }

        public void Reset()
        {
            // i should add a fade out to the music playing but that would require getting access to the GLOBAL sound manager and do it there, not too fond of that
            stateManager.GameplayMachine.CurrentState.SetDefault();
            screenStack.ScaleTo(Vector2.Zero, 1000D, Easing.OutQuint).FadeOutFromOne(800D, Easing.OutQuint).OnComplete(_ =>
            {
                Container maskingContainer = (Container)screenStack.Parent;
                maskingContainer!.Remove(screenStack, true);
                maskingContainer.Add(screenStack = createScreenStack());
            });
        }

        private void enableBacking()
        {
            stateManager.ProgressionBlock.SetDefault();
            stateManager.CanBack.SetDefault();
        }

        private SweetScreenStack createScreenStack() => new()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Alpha = 0,
            Scale = new Vector2(0.8F), // for da transition (not entirely visible actually)
        };

        // this is probably better than the previous fix, still doesnt meet the standards but works better
        internal static event Action Disposed;
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Disposed?.Invoke();
        }
    }
}
