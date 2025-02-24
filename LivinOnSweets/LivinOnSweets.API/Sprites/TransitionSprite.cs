using JetBrains.Annotations;
using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Extensions;
using LivinOnSweets.API.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    // Used for the transition animations, includes the circular wiping transition, this sprite should sit on top of the parent container
    // TODO: Fix random blinking on some parts of the transitions
    // they randomly happen, most likely to be the circular wipe shader but I don't really know
    public partial class TransitionSprite : CompositeDrawable
    {
        private bool useReisa;
        private bool wasPreload;

        public ScreenStack TargetScStack;
        public SweetScreen NextScreen;

        // Mochi
        private CircularWipe bgCover;
        private CircularWipe mochiCover;

        // For both Reisa and the Mochi animations
        private CircularWipe transitionMask;
        private ScreenStack fakeStack;

        public TransitionSprite(bool forceSpecial = false, bool preloading = false)
        {
            if (forceSpecial)
                useReisa = true;

            if (!forceSpecial && !preloading)
                useReisa = RNG.NextBool(0.25D);

            wasPreload = preloading;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (wasPreload)
            {
                // Only preload the animation instead of creating all of the wipes n shit
                InternalChild = (useReisa) ? new ReisaAnimation(null) : new MochiAnimation(null);
                return;
            }

            fakeStack = genScreenStack();

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;

            // I need to load the next screen UNDER this and then when finished change the context to the screenstack apparently
            transitionMask = new CircularWipe()
            {
                Reveal = false,
                Progress = 0,

                Colour = Colour4.Transparent, // TODO: Find a way to wipe when the color is transparent
                Alpha = 0,

                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            Drawable animation = (useReisa) ? new ReisaAnimation(changeReady) : new MochiAnimation(changeReady);
            if (useReisa)
            {
                InternalChildren =
                [
                    animation, // reisa animation
                    fakeStack, // the screen stack
                    transitionMask, // the master transition
                ];
            }
            else
            {
                bgCover = new CircularWipe()
                {
                    Reveal = true,
                    Progress = 0,

                    Colour = Colour4.White,
                    Alpha = 0,
                    Depth = 4,

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                mochiCover = new CircularWipe()
                {
                    Reveal = true,
                    Progress = 0,

                    Colour = Colour4.FromHex("#80ffd0"),
                    Alpha = 0,
                    Depth = 2,

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                InternalChildren =
                [
                    bgCover, // covers the background
                    animation, // mochi animation
                    mochiCover, // covers the mochi animation
                    fakeStack, // on top of everything the fake stack
                    transitionMask // on top the transition mask used to reveal the fake stack
                ];
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // chill like that
            if (wasPreload)
            {
                removeFromParent();
                return;
            }

            // in order to keep a perfect circle in the shader, we set the size to be like a quad not a rect
            Vector2 wipeSize = new Vector2(Parent!.DrawWidth);
            if (!useReisa)
                bgCover.Size = mochiCover.Size = wipeSize;

            transitionMask.Size = wipeSize;
        }

        private void changeReady()
        {
            // we need a target screen stack in here lad
            if (TargetScStack == null)
                throw new InvalidOperationException();

            double startMask = 0D;
            Colour4 nextColor = Colour4.White;
            if (!useReisa)
            {
                bgCover.Alpha = 1;
                mochiCover.Alpha = 1;

                bgCover.TransformTo("Progress", 1f, 800D);

                double delay = 400D;
                mochiCover
                    .Delay(delay)
                    .TransformTo("Progress", 1f, 800D);

                nextColor = mochiCover.Colour;

                // - delay since we only want to take the progress transform duration
                startMask = (mochiCover.LatestTransformEndTime - mochiCover.TransformStartTime) - delay;
            }

            // switch the screen context here i suppose
            transitionMask
                .Delay(startMask)
                .FadeInFromZero() // This may cause blinking
                .FadeColour(nextColor)
                .TransformTo("Progress", 1f, 800D)
                .OnComplete(_ => Schedule(switchContext));

            fakeStack
                .Delay(startMask)
                .FadeInFromZero();
        }

        private ScreenStack genScreenStack()
        {
            if (!wasPreload && NextScreen == null)
                throw new InvalidOperationException();
            else if (wasPreload && NextScreen == null)
                NextScreen = new SweetScreen();

            ScreenStack stack = new ScreenStack()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Depth = 1
            };

            stack.Push(NextScreen);

            return stack;
        }

        // I spent like one hour fr trying to decouple the parent and shit while I just had to schedule the call :skull:
        private void switchContext()
        {
            // SWITCH!
            fakeStack.Remove(NextScreen, false);
            RemoveInternal(fakeStack, false);
            fakeStack = null;

            NextScreen.ChangeLoadState(LoadState.Ready);
            TargetScStack.Push(NextScreen);

            // Remove the transition from the current screen or container when done
            ScheduleAfterChildren(removeFromParent);
        }

        private void removeFromParent()
        {
            if (Parent is Container pContainer)
            {
                pContainer.Remove(this, false);
                return;
            }

            if (Parent is SweetScreen pScreen)
            {
                pScreen.Remove(this, true);
                return;
            }
        }

        private partial class MochiAnimation : TextureAnimation
        {
            private bool invoked;
            [CanBeNull] private Action animFinished;

            public MochiAnimation([CanBeNull] Action onFinish)
            {
                animFinished = onFinish;
                Depth = 3;
            }

            [BackgroundDependencyLoader]
            private void load(MainMenuStore mmStore)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                AddFrames(mmStore.GetFrames("MainMenu/Transitions/Mochi.png", 60D, 1, 7));
                Loop = false;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (!invoked && PlaybackPosition >= Duration)
                {
                    animFinished?.Invoke();
                    invoked = true;
                }
            }
        }

        private partial class ReisaAnimation : Container
        {
            [CanBeNull] private Action animFinished;
            private Sprite fling;
            private Sprite hit;

            public ReisaAnimation([CanBeNull] Action onFinish)
            {
                animFinished = onFinish;
                Depth = 2;
            }

            [BackgroundDependencyLoader]
            private void load(MainMenuStore mmStore)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Origin = Anchor.Centre;

                Add(fling = new Sprite()
                {
                    Texture = mmStore.Get("MainMenu/Transitions/ReisaFling.png"),
                    Scale = new Vector2(0.75f),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                });

                Add(hit = new Sprite()
                {
                    Texture = mmStore.Get("MainMenu/Transitions/ReisaScreen.png"),
                    Alpha = 0,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Scale = new Vector2(0.75f)
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (animFinished == null)
                    return;

                fling.Y = DrawHeight + 350;

                animate();
            }

            // Although it doesn't really replicate the og animation, I kinda like how it turned out, took me some hours again..
            // TODO: Adjust timings again
            private void animate()
            {
                fling
                    .MoveToY(-50, 500D, Easing.OutCubic)
                    .Then()
                    .MoveToY((DrawHeight / 2) - (fling.DrawHeight / 2), 500D, Easing.OutCubic)
                    .Delay(250D)
                    .FadeOut();

                fling.Delay(300D).ScaleTo(2, 6500D, Easing.OutElastic);

                hit
                    .Delay(550D)
                    .FadeIn()
                    .ScaleTo(1.005f, 500D, Easing.OutBack)
                    .Delay(300D)
                    .MoveToY(100, 800D, Easing.OutCubic)
                    .Then()
                    .MoveToY(300, 500D)
                    .Then()
                    .MoveToY(750, 1000D, Easing.OutCubic)
                    .Then()
                    .MoveToY(hit.DrawHeight, 1000D, Easing.OutCubic) // Should start transitioning here
                    .OnComplete(_ => hit.FadeOut(500D, Easing.OutQuint));

                double hitTime = hit.LatestTransformEndTime - hit.TransformStartTime;
                Scheduler.AddDelayed(animFinished!, hitTime);
            }
        }
    }
}
