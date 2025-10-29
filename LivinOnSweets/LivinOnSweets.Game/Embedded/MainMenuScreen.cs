using System;
using System.Collections.Generic;
using LivinOnSweets.API.Audio;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Graphics.Sprites;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.Skinning;
using LivinOnSweets.API.StateMachines;
using LivinOnSweets.API.Utils;
using LivinOnSweets.Game.Embedded.SubScreens;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Screens;
using osuTK.Input;

namespace LivinOnSweets.Game.Embedded
{
    // much better than https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.Game/GameScreens/MainMenuScreen.cs
    public partial class MainMenuScreen : SweetScreen, IKeyBindingHandler<ManiaAction>
    {
        [Resolved] private LoadManager loadManager { get; set; }
        [Resolved] private GameStateManager stateManager { get; set; }

        public override bool DragBlocksClick => true;

        private Container<SlideElement> slideElements;
        private Container<CharacterParallaxBackground> backgrounds;

        // tracks the screens that were already loaded and dont need another load
        // even if we re-create an instance when going to it, the needed stuff (textures mostly) have been already loaded
        private readonly List<Type> loadedTypes = [];

        private DrawableTrack bgMusic;
        private readonly Box fadeOverlay = new()
        {
            RelativeSizeAxes = Axes.Both,
            Colour = Colour4.Black,
            Depth = -99
        };

        private bool selected;
        private int curSelected;

        [BackgroundDependencyLoader]
        private void load(ResourcePackManager packManager)
        {
            AddRangeInternal([
                backgrounds = new Container<CharacterParallaxBackground>()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                slideElements = new Container<SlideElement>()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                }
            ]);

            IResourcePack pack = packManager.CurrentPack.Value;
            MenuEntryInfo menuEntries = TomlMenuParser.ParseToml(packManager, pack);
            foreach (MenuEntryInfo.EntryInfo menuEntry in menuEntries.Entries)
                loadManager.Register(new CharacterParallaxBackground(menuEntry, ref slideElements), t =>
                {
                    t.Hide();
                    t.FinishTransforms(true);
                    backgrounds.Add(t);
                });

            // YES the menu music is bluemark canvas, YES i copied it to the main menu folder to make the bg music easier to replace
            ITrack track = pack.GetTrack(new BackgroundInfo());
            if (track != null)
            {
                bgMusic = new DrawableTrack((Track)track);
                bgMusic.Looping = true;
                loadManager.Register(bgMusic, AddInternal);
            }

            // are we really gonna load a simple box?
            AddInternal(fadeOverlay);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            fadeOverlay.FadeOutFromOne(1000D, Easing.OutQuint);

            bgMusic.Volume.Value = 0;
            bgMusic.Start();
            this.TransformBindableTo(bgMusic.Volume, bgMusic.Volume.Default, 300D);

            for (int i = 0; i < backgrounds.Count; i++)
            {
                CharacterParallaxBackground background = backgrounds[i];
                if (i != curSelected)
                    background.Hide();
                else
                    background.Show();
            }
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            if (e.Last is SweetSubScreen)
            {
                selected = false;
                return;
            }

            fadeOverlay.FadeOutFromOne(1000D, Easing.OutQuint);

            selected = false;
            bgMusic.Start();
            this.TransformBindableTo(bgMusic.Volume, bgMusic.Volume.Default, 300D);
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (!backgrounds[curSelected].FinishedTransform || selected)
                return false;

            switch (e.Action)
            {
                case ManiaAction.UI_RIGHT:
                    changeSelected(1);
                    return true;

                case ManiaAction.UI_LEFT:
                    changeSelected(-1);
                    return true;

                case ManiaAction.CONFIRM:
                    selected = true;
                    switch (backgrounds[curSelected].Id)
                    {
                        // we setting the state machines BEFORE finishing the load to avoid backing out middle load
                        case "play":
                            stateManager.GameplayMachine.CurrentState.Value = GameplayState.SongSelect;
                            preloadNext<SongSelectScreen>(true, ScreenStack.Push);
                            break;

                        case "story":
                            stateManager.GameplayMachine.CurrentState.Value = GameplayState.StorySelect;
                            preloadNext<StoryLobbyScreen>(true, ScreenStack.Push);
                            break;

                        case "options":
                            stateManager.GameplayMachine.CurrentState.Value = GameplayState.GameOptions;
                            preloadNext<OptionsScreen>(false, (sc) =>
                            {
                                SweetSubScreen screen = sc as SweetSubScreen;
                                ScreenStack.PushSubScreen(screen);
                                // i dont really like this but uhh aight ill use whatever i can
                                stateManager.GameplayMachine.SetOnExit(GameplayState.GameOptions, () => resumeOnExitSubScreen(GameplayState.GameOptions, screen));
                            });
                            break;
                    }
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        private void resumeOnExitSubScreen(GameplayState onExitState, SweetSubScreen exitScreen)
        {
            OnResuming(new ScreenTransitionEvent(exitScreen, this));
            stateManager.GameplayMachine.SetOnExit(onExitState, null);
        }

        // this is plain old code, should improve it someday lol!!
        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!backgrounds[curSelected].FinishedTransform || e.Button != MouseButton.Left || selected)
                return false;

            changeSelected(e.Delta.X > 5 ? 1 : -1);

            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            // Send a fake keypress, to avoid duplicating the code? since the confirm switch case already handles that
            OnPressed(new KeyBindingPressEvent<ManiaAction>(new InputState(), ManiaAction.CONFIRM));
            return true;
        }

        private void changeSelected(int value)
        {
            int prevIndex = curSelected;
            curSelected = (curSelected + value).Wrap(backgrounds.Count);

            backgrounds[prevIndex].FadeOut(value);
            backgrounds[curSelected].FadeIn(value);
        }

        // OMG OMG OMG OLD CODE AGAIN?!?!?!? YESS I LOVE IT!!
        private void preloadNext<T>(bool isFadeTransition, Action<SweetScreen> onLoaded)
        {
            Type screenType = typeof(T);
            bool shouldAnimate = !loadedTypes.Contains(screenType);
            if (shouldAnimate)
                fadeOverlay.FadeTo(0.8F, 500D, Easing.OutQuint);

            // reusing stuff cuz its meant to
            GameScreenData screenData = new GameScreenData(screenType);
            SweetScreen nextScreen = screenData.CreateScreen();
            if (nextScreen == null)
            {
                stateManager.GameplayMachine.UpdateState(true); // backing here since we already set the state before
                fadeOverlay.FadeOut(500D, Easing.OutQuint);
                selected = false;
                return;
            }

            if (!shouldAnimate)
            {
                loadFinish();
                return;
            }

            LoadComponentAsync(nextScreen, _ =>
            {
                loadedTypes.Add(screenType);
                loadFinish();
            });
            return;

            void loadFinish()
            {
                if (screenData.IsSubScreen() && !shouldAnimate)
                {
                    onLoaded(nextScreen);
                    return;
                }

                if (!screenData.IsSubScreen())
                    this.TransformBindableTo(bgMusic.Volume, 0, 300D).OnComplete(_ => bgMusic.Stop());

                if (isFadeTransition)
                    fadeOverlay.FadeIn(500D, Easing.OutQuint).OnComplete(_ => onLoaded(nextScreen));
                else
                    fadeOverlay.FadeOut(500D, Easing.OutQuint).OnComplete(_ => onLoaded(nextScreen));
            }
        }

        // temp shi
        private class BackgroundInfo : IAudioInfo
        {
            public IEnumerable<string> LookupNames => ["MainMenu/bgm", "MainMenu/bg_music", "MainMenu/menu"];
            public int Volume => 100;
        }
    }
}
