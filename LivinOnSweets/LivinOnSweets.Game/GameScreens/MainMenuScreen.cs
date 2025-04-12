using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Interfaces;
using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace LivinOnSweets.Game.GameScreens
{
    // TODO: Save up the current selected entry for next runs
    public partial class MainMenuScreen : SweetScreen, IProgressReporter, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private LoadingSpinner gcSpinner { get; set; }

        [Resolved]
        private GameStateManager stateManager { get; set; }

        private List<Drawable> loadTargets = [];

        // Saves the screen types that were preloaded
        private List<Type> preloadedScreens = [];

        protected Container Content;
        protected DrawableTrack BgMusic;
        private Box fadeOverlay;

        protected Container<CharacterParallaxBackground> Backgrounds;
        private int curSelected;

        protected int CurSelected
        {
            get => curSelected;
            set
            {
                // Get the previous and new selection indices
                int prevIndex = curSelected;
                curSelected = (curSelected + value).Wrap(Backgrounds.Count);

                Backgrounds[prevIndex].FadeOut(value);
                Backgrounds[curSelected].FadeIn(value);
            }
        }

        public MainMenuScreen()
        {
            InternalChild = Content = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
            };

            Content.Add(Backgrounds = new AutoSizeOnceContainer<CharacterParallaxBackground>(Axes.Both)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both
            });
        }

        [BackgroundDependencyLoader]
        private void load(RhythmGameStore rhythmStore)
        {
            BgMusic = new DrawableTrack(rhythmStore.TrackStore.Get("RhythmGame/Songs/Bluemark Canvas.mp3"));
            BgMusic.Looping = true;
            Content.Add(BgMusic);

            CharacterParallaxBackground[] bgs =
            [
                new(MainMenuEntry.PLAY),
                new(MainMenuEntry.OPTION),
                new(MainMenuEntry.STORY)
            ];
            Backgrounds.AddRange(bgs);

            Content.Add(fadeOverlay = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black,
                Depth = -99
            });

            TransitionSprite[] transitions = [new(preloading: true), new(true, true)];
            Content.AddRange(transitions);

            lock (loadLock)
            {
                loadTargets.Add(BgMusic);

                loadTargets.AddRange(bgs);
                foreach (CharacterParallaxBackground bg in bgs)
                {
                    loadTargets.AddRange(bg.Children);
                }

                loadTargets.AddRange(transitions);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (!Backgrounds[CurSelected].FinishedTransform || selected)
                return false;

            bool handled = false;
            switch (e.Action)
            {
                case ManiaAction.UI_RIGHT:
                    CurSelected = 1;
                    handled = true;
                    break;

                case ManiaAction.UI_LEFT:
                    CurSelected = -1;
                    handled = true;
                    break;

                case ManiaAction.CONFIRM:
                    handled = true;
                    break;
            }

            return handled;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        // I would like to add real dragging, like holding down the input and being able to move the slide but it sounds kinda hard
        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!Backgrounds[CurSelected].FinishedTransform || e.Button != MouseButton.Left)
                return false;

            if (e.Delta.X > 1)
                CurSelected = 1;
            else
                CurSelected = -1;

            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            // Send a fake keypress, to avoid duplicating the code? since the confirm switch case already handles that
            OnPressed(new KeyBindingPressEvent<ManiaAction>(new InputState(), ManiaAction.CONFIRM));
            return true;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            lock (loadLock)
            {
                loadTargets = null;
            }

            fadeOverlay.FadeOutFromOne(1000D, Easing.OutQuint);

            BgMusic.Volume.Value = 0;
            BgMusic.Start();
            this.TransformBindableTo(BgMusic.Volume, BgMusic.Volume.Default, 300D);

            for (int i = 0; i < Backgrounds.Count; i++)
            {
                CharacterParallaxBackground background = Backgrounds[i];
                if (i != curSelected)
                    background.Hide();
                else
                    background.Show();
            }
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            selected = false;
            fadeOverlay.FadeOutFromOne(1000D, Easing.OutQuint);

            BgMusic.Start();
            this.TransformBindableTo(BgMusic.Volume, BgMusic.Volume.Default, 300D);
        }

        private void preloadNext(Type screenType, Action<SweetScreen> loaded)
        {
            bool shouldAnimate = !preloadedScreens.Contains(screenType);

            if (shouldAnimate)
                fadeOverlay.FadeTo(0.75f, 500D, Easing.OutQuint);

            // create an underlying game screen data object cuz im extremely lazy to use reflect and more shit yknow
            GameScreenData screenData = new GameScreenData(screenType);

            SweetScreen nextScreen = screenData.CreateScreen();
            if (nextScreen == null)
            {
                fadeOverlay.FadeOut(500D, Easing.OutQuint);
                return;
            }

            if (!shouldAnimate)
            {
                loaded(nextScreen);
                return;
            }

            // Only show the spinner when loading
            gcSpinner.Show();
            Scheduler.AddDelayed(loadScreen, 1000D); // delay it a little bit hehe :grin:

            void loadScreen()
            {
                LoadComponentAsync(nextScreen, _ =>
                {
                    preloadedScreens.Add(screenType);
                    gcSpinner.Hide();
                    fadeOverlay.FadeOut(500D, Easing.OutQuint).OnComplete(_ => loaded(nextScreen));
                });
            }
        }

        // Lock object to prevent mutation exceptions (loadTargets gets modified on load and the function gets called asap)
        private object loadLock = new();
        float IProgressReporter.GetLoadProgress()
        {
            lock (loadLock)
            {
                float loaded = loadTargets.Count(t => t.LoadState == LoadState.Ready);
                float total = loadTargets.Count;
                return loaded / total;
            }
        }
    }
}
