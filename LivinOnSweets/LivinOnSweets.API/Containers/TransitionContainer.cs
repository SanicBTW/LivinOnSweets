using AetherFramework;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Events;
using LivinOnSweets.API.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;

namespace LivinOnSweets.API.Containers
{
    public partial class TransitionContainer : DrawSizePreservingFillContainer
    {
        private ScreenStack screenStack => (ScreenStack)Children[0];

        [BackgroundDependencyLoader]
        private void load()
        {
            EventManager.Register<TransitionEvent>(EventRegistryType.GLOBAL, handleTransition);

            // Load settings to uncap the strategy?
            RelativeSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Has the screenstack already, we want the screen stack to be under the future sprites
            ChangeChildDepth(screenStack, 1);
        }

        private void handleTransition(TransitionEvent ev)
        {
            // Most likely handled by another mod?
            if (ev.IsCancelled)
                return;

            ScreenTransitionType transitionType = ev.TransitionData.TransitionType;
            switch (transitionType)
            {
                case ScreenTransitionType.FADE fadeData:
                    makeFade(fadeData);
                    // Push the next screen after fade in
                    Scheduler.AddDelayed(() => screenStack.Push(ev.TransitionData.NextScreen), fadeData.FadeInDuration);
                    break;

                case ScreenTransitionType.SPRITE_ANIMATED:
                    Add(new TransitionSprite(true)
                    {
                        TargetScStack = screenStack,
                        NextScreen = ev.TransitionData.NextScreen
                    });
                    break;
            }
        }

        private void makeFade(ScreenTransitionType.FADE fadeData)
        {
            Box overlay = new Box()
            {
                RelativeSizeAxes = Axes.Both,
                Colour = fadeData.Color,
                Alpha = 0,
            };

            Add(overlay);

            overlay.Delay(100D)
                .FadeInFromZero(fadeData.FadeInDuration, fadeData.Easing)
                .Then()
                .FadeOutFromOne(fadeData.FadeOutDuration, fadeData.Easing)
                .Then()
                .OnComplete(_ => Remove(overlay, true));
        }
    }
}
