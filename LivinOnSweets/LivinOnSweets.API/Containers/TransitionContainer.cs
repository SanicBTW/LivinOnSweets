using AetherFramework;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Events;
using LivinOnSweets.API.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
                // TODO
                case ScreenTransitionType.BASIC_FADE:
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
    }
}
