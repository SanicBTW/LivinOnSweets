using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Data;
using LivinOnSweets.API.Enum;
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
        private List<Drawable> loadTargets = [];

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
                curSelected += value;
                wrap(ref curSelected, Backgrounds.Count);

                if (prevIndex == curSelected) return; // Prevent unnecessary updates

                CharacterParallaxBackground prevSel = Backgrounds[prevIndex];
                CharacterParallaxBackground curSel = Backgrounds[curSelected];

                int nextEntry = curSelected + value;
                wrap(ref nextEntry, Backgrounds.Count);
                CharacterParallaxBackground nextSel = Backgrounds[nextEntry];

                if (prevSel == curSel || curSel == nextSel || prevSel == nextSel)
                    throw new UnreachableException();

                bool slidingLeft = int.IsNegative(value);

                // Slide out the previous entry
                prevSel.SlideOut(slidingLeft);

                // Slide in the new entry
                curSel.SlideIn();

                // Set the offscreen background to the correct position
                nextSel.SlideOffscreen(slidingLeft);
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

            Content.Add(Backgrounds = new Container<CharacterParallaxBackground>()
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

            lock (loadLock)
            {
                loadTargets.Add(BgMusic);
                loadTargets.AddRange(bgs);
            }

            Content.Add(fadeOverlay = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black,
                Depth = -99
            });
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();

            // This is a hack I learnt while doing the editor, check ToolBar.cs
            if (Backgrounds.AutoSizeAxes.HasFlagFast(Axes.Both))
            {
                Vector2 prevSize = Backgrounds.DrawSize;
                Backgrounds.AutoSizeAxes = Axes.None;
                Backgrounds.Size = prevSize;
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (!Backgrounds[CurSelected].FinishedTransform)
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

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            fadeOverlay.FadeOutFromOne(1000D, Easing.OutQuint);

            RepositionBackgrounds();

            BgMusic.Volume.Value = 0;
            BgMusic.Start();
            this.TransformBindableTo(BgMusic.Volume, BgMusic.Volume.Default, 200D);
        }

        protected virtual void RepositionBackgrounds()
        {
            for (int i = 0; i < Backgrounds.Count; i++)
            {
                if (i == CurSelected)
                    Backgrounds[i].MoveToX(0);
                else if (i == (CurSelected - 1 + Backgrounds.Count) % Backgrounds.Count)
                    Backgrounds[i].SlideOffscreen(true);
                else if (i == (CurSelected + 1) % Backgrounds.Count)
                    Backgrounds[i].SlideOffscreen(false);
                else
                    Backgrounds[i].SlideOffscreen(true);
            }
        }

        private void wrap(ref int value, int totalCount)
        {
            if (totalCount <= 0)
                value = 0;

            if (value < 0)
                value = totalCount - 1;

            if (value >= totalCount)
                value = 0;
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
