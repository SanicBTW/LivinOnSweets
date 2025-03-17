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
    // TODO: If the GPU is busy on the frame, blinking might happen
    // I managed to reduce the probability even more but now it depends on the GPU or ME
    public partial class TransitionSprite : CompositeDrawable
    {
        private bool useReisa;
        private bool wasPreload;

        public ScreenStack TargetScStack;
        public SweetScreen NextScreen;

        private Drawable sprAnimation;

        // Mochi
        private CircularWipe bgCover;
        private CircularWipe mochiCover;

        // Reisa
        private CircularWipe reisaMask;

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

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;

            reisaMask = new CircularWipe()
            {
                Reveal = false,
                Progress = 0,

                Colour = Colour4.White, // TODO: Find a way to wipe when the color is transparent
                Alpha = 0,

                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            sprAnimation = (useReisa) ? new ReisaAnimation(changeReady) : new MochiAnimation(changeReady);
            if (useReisa)
            {
                InternalChildren =
                [
                    sprAnimation, // reisa animation
                    reisaMask, // the master transition
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
                    sprAnimation, // mochi animation
                    mochiCover, // covers the mochi animation
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

            reisaMask.Size = wipeSize;
        }

        private void changeReady()
        {
            // we need a target screen stack in here lad
            if (TargetScStack == null)
                throw new InvalidOperationException();

            if (!useReisa)
            {
                bgCover.Alpha = 1;
                mochiCover.Alpha = 1;

                bgCover.TransformTo("Progress", 1f, 800D);

                double delay = 400D;
                mochiCover
                    .Delay(delay)
                    .TransformTo("Progress", 1f, 800D);

                double maskTime = (mochiCover.LatestTransformEndTime - mochiCover.TransformStartTime) - delay / 2;

                Scheduler.AddDelayed(switchContext, maskTime);
            }
            else
            {
                // when the transition finishes, clean it up and schedule the screen changes as well as the removal of ourselves from the parent container
                reisaMask
                    .FadeInFromZero()
                    .TransformTo("Progress", 1f, 800D)
                    .OnComplete(_ => Schedule(switchContext));

                double maskTime = (reisaMask.LatestTransformEndTime - reisaMask.TransformStartTime);

                Schedule(() => TargetScStack.Push(NextScreen));
                Scheduler.AddDelayed(removeFromParent, maskTime);
            }
        }

        // I spent like one hour fr trying to decouple the parent and shit while I just had to schedule the call :skull:
        private void switchContext()
        {
            RemoveInternal(sprAnimation, true);

            if (!useReisa)
            {
                RemoveInternal(bgCover, true);

                mochiCover.Reveal = false;
                mochiCover.TransformTo("Progress", 0f)
                    .TransformTo("Progress", 1f, 800D)
                    .OnComplete(_ => removeFromParent()); // remove from the parent once its finished

                TargetScStack.Push(NextScreen);
            }
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
                pScreen.RemoveInternal(this, false);
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
