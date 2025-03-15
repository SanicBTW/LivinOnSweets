using AetherFramework.Events;
using LivinOnSweets.API.Data;

namespace LivinOnSweets.API.Events
{
    public class TransitionEvent : TargetedEvent
    {
        public readonly ScreenTransitionData TransitionData;

        public TransitionEvent(ScreenTransitionData transitionData) : base(null)
        {
            TransitionData = transitionData;
        }
    }
}
