using System;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Cursor;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Graphics.Containers;
using LivinOnSweets.API.Graphics.Sprites;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.StateMachines;
using LivinOnSweets.Game.GameScreens;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.Shell
{
    // The rewritten version of the startup screen, the one the rewrite really needed
    // this is the 3rd revision of the whole screening system, it better be good enough bruh
    public partial class ReStartupScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved] private GameStateManager stateManager { get; set; }
        [Resolved] private GameSession gameSession { get; set; }

        private Box transitionFade;
        private SweetScrollContainer scrollContainer;

        private StartupBackground background;
        private StartupDisc disc;
        private Box focusOverlay;
        private GameFrameContainer gameFrame;

        private Bindable<bool> noContextChange;
        private bool skippedDisclaimer;

        private bool transitioning;

        // values provided from good ol' me
        private const double game_anim_delay = 1250;
        private const float anim_offset = 150;
        private double lastScrollPos;

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack, SweetConfigManager sweetConfig)
        {
            noContextChange = sweetConfig.GetBindable<bool>(SweetSetting.NoGameplayContextChange);
            skippedDisclaimer = sweetConfig.Get<bool>(SweetSetting.SkipProjectDisclaimer);

            Texture brandTexture = pack.GetTexture("Startup/UI/Branding.png");
            brandTexture.ScaleAdjust = 1;
            SweetContextMenuContainer ctxMenuContainer = new SweetContextMenuContainer()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    // TODO: temp fix lol sorry
                    // I know its gonna look weird when everything except the damn background scrolls but ive spent a moderate amount of time figuring out
                    // how to disable the scaling for this sprite only with proper sizing inside the scroll container, another issue is
                    // scrolling outside of the bounds, which should be fixed by setting clamp ext to 0 but i dont want to do it cuz it looks cool
                    background = new StartupBackground(),
                    scrollContainer = new SweetScrollContainer(Direction.Vertical, true)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        ClampExtension = 40,
                        Children =
                        [
                            disc = new StartupDisc(),
                            focusOverlay = new Box { RelativeSizeAxes = Axes.Both, Colour = Colour4.Black, Alpha = 0f },
                            gameFrame = new GameFrameContainer(() =>
                                OnPressed(new KeyBindingPressEvent<ManiaAction>(new InputState(), ManiaAction.CONFIRM)), true),
                            new Sprite
                            {
                                Origin = Anchor.TopLeft,
                                Anchor = Anchor.TopLeft,
                                Margin = new MarginPadding
                                {
                                    Left = 28,
                                    Top = 32
                                },
                                Texture = brandTexture,
                            }
                        ]
                    },
                    transitionFade = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black,
                    }
                ]
            };

            AddInternal(ctxMenuContainer);

            // if the preloader skipped the disclaimer then theres only a black screen to transition from
            // and we want to make it look seamless if possible in both cases
            scrollContainer.Alpha = skippedDisclaimer ? 1 : 0;
            transitionFade.Alpha = scrollContainer.Alpha;
            background.Alpha = scrollContainer.Alpha;

            // setting callbacks here since they wont be able to be executed until progression block is lifted
            // stateManager.RuntimeMachine.SetOnEnter(RuntimeState.ClosePrompt, null); // entering to close prompt from startup
            stateManager.RuntimeMachine.SetOnEnter(RuntimeState.InGame, focusGame); // entering to in game from startup
            stateManager.RuntimeMachine.SetOnExit(RuntimeState.InGame, unfocusGame); // exiting from in game to startup
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            stateManager.ProgressionBlock.Value = true;

            double fadeDuration = 1200D;
            if (skippedDisclaimer)
            {
                transitionFade.FadeOut(fadeDuration, Easing.OutQuint);
            }
            else
            {
                scrollContainer.FadeIn(fadeDuration, Easing.OutQuint);
                background.FadeIn(fadeDuration, Easing.OutQuint);
            }

            gameFrame.MoveToOffset(new Vector2(0, anim_offset)).FadeTo(0); // cancels the parent fade animation

            using (BeginDelayedSequence(500))
            {
                gameFrame
                    .FadeIn(game_anim_delay)
                    .MoveToOffset(new Vector2(0, -anim_offset), game_anim_delay, Easing.OutQuint);

                double endDelay = game_anim_delay + (game_anim_delay / 5);
                using (BeginDelayedSequence(endDelay))
                {
                    disc.Slide();
                    endDelay += TransformDelay / 2;
                }

                Scheduler.AddDelayed(() =>
                {
                    gameFrame.Show();
                    using (BeginDelayedSequence(250D))
                        gameFrame.ShowEnterZone();

                    // bring back the scrollbar effects once its fixed
                    scrollContainer.ScrollBarColour.Value = Colour4.Black;
                    scrollContainer.ScrollBlocked.Value = false;

                    stateManager.ProgressionBlock.SetDefault();
                }, endDelay);
            }
        }

        private void focusGame()
        {
            // already in the middle of transition, dont trigger more
            if (transitioning)
                return;

            transitioning = true;

            stateManager.CanBack.Value = false; // First run should wait for the container to fully load
            stateManager.ProgressionBlock.Value = true; // Block any possible progression

            disc.Slide(false);
            if (!noContextChange.Value)
            {
                gameFrame.Hide(); // hide the banners when doing the context change

                lastScrollPos = scrollContainer.Current;
                scrollContainer.ScrollBlocked.Value = true; // we want to allow scrolling when we dont change the context
            }

            double framePos = scrollContainer.GetChildPosInContent(gameFrame);
            scrollContainer.ScrollTo(framePos * 1.5F); // this looks decent really

            gameFrame.HideEnterZone();
            if (noContextChange.Value)
                loadGame();
            else
            {
                transitionFade.Delay(500).FadeInFromZero(game_anim_delay, Easing.OutQuint)
                    .OnComplete(_ => Scheduler.AddDelayed(loadGame, game_anim_delay / 5));

                Scheduler.AddDelayed(() => PushSubScreen(new SakurakoTransitSub()), game_anim_delay / 4);
            }
        }

        private void unfocusGame()
        {
            if (noContextChange.Value)
                resetProps();
            else
            {
                // when changing context, the game frame lost ownership so we try to get it back, already setting some properties back
                gameFrame.ResetOwnership();

                transitionFade.Delay(500).FadeOutFromOne(game_anim_delay / 5, Easing.OutQuint)
                    .OnComplete(_ => resetProps());
            }
            return;

            void resetProps()
            {
                // disable input no matter the context
                gameSession.InputEnabled.Value = false;

                stateManager.CanBack.SetDefault();
                stateManager.ProgressionBlock.SetDefault();

                scrollContainer.ScrollBlocked.Value = false;
                scrollContainer.ScrollTo(lastScrollPos);

                disc.Slide();
                gameFrame.Show();
                gameFrame.ShowEnterZone();

                if (noContextChange.Value)
                {
                    gameSession.RequestGameFocus(false);
                    focusOverlay.FadeOut(500D, Easing.OutQuint);
                    gameFrame.FadeBannersTo(1, 500D, Easing.OutQuint);
                }
            }
        }

        private void loadGame()
        {
            bool notInit = stateManager.GameplayMachine.CurrentState.Value == GameplayState.NotReady;
            // Since the screens are part of the game and not the API package we pass a type reference to the next screen that will be created thru activator
            // When entering the game, let the game load first then after its done loading, change the current screen
            Type screenType = notInit ? typeof(PlayStateTest) : null;
            gameFrame.PushScreen(new GameScreenData(screenType, onLoad: onLoad, onError: onError));
            return;

            void onError() => stateManager.RuntimeMachine.UpdateState(true);

            void onLoad()
            {
                transitioning = false;

                if (noContextChange.Value)
                {
                    gameSession.InputEnabled.Value = true;
                    gameSession.RequestGameFocus();
                    focusOverlay.FadeTo(0.75F, 500D, Easing.OutQuint);
                    gameFrame.FadeBannersTo(0.5F, 500D, Easing.OutQuint);
                    return;
                }

                if (IsSubScreenOpen)
                    ExitSubScreen();

                ScreenStack.Push(new FullscreenSession());
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            switch (e.Action)
            {
                case ManiaAction.CONFIRM:
                    if (e.Repeat)
                        return false;

                    // updates into in game
                    stateManager.RuntimeMachine.UpdateState(false);
                    return true;

                case ManiaAction.BACK:
                    if (e.Repeat) { } // hold to close (?)
                    else
                    {
                        RuntimeState runtimeState = stateManager.RuntimeMachine.CurrentState.Value;

                        if (!noContextChange.Value || runtimeState != RuntimeState.InGame)
                            return false;

                        GameplayState gameState = stateManager.GameplayMachine.CurrentState.Value;
                        if (gameState > GameplayState.Ready) // if the state is above ready (which also includes not ready) then return
                            return false;

                        // from in game to startup, by default when backing into it (startup) focus game should be called cuz we are listening to that exit
                        stateManager.RuntimeMachine.UpdateState(true);
                    }
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private partial class SakurakoTransitSub : SweetSubScreen
        {
            private SakurakoTransition transit;

            public SakurakoTransitSub()
            {
                InternalChild = transit = new SakurakoTransition();
            }

            public override bool OnExiting(ScreenExitEvent e)
            {
                // waits for the diff of time between the real last time and the current end time
                double timeDiff = (transit.LastEndTime - transit.AnimationTrack.AnimationEnd) * 1000;
                Scheduler.AddDelayed(Exit, timeDiff);

                // sets the track time to the end time and sets the end time to the real end time
                transit.AnimationTrack.TrackTime = transit.AnimationTrack.AnimationEnd;
                transit.AnimationTrack.AnimationEnd = transit.LastEndTime;

                // it will show as complete until setting the track time, which then returns false and we setting it to true
                // so the first exit call is blocked until the next scheduled one
                return !transit.AnimationTrack.IsComplete;
            }
        }
    }
}
