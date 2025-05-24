using osu.Framework.Bindables;

namespace LivinOnSweets.API.StateMachines
{
    public class StateMachine<TState> where TState : struct, Enum
    {
        public Bindable<TState> CurrentState { get; } = new();

        private readonly Dictionary<TState, Func<bool, TState?>> transitions = new();
        private readonly Dictionary<TState, Action> onEnterActions = new();
        private readonly Dictionary<TState, Action> onExitActions = new();

        public void SetTransition(TState from, Func<bool, TState?> transitionLogic)
        {
            transitions[from] = transitionLogic;
        }

        public void SetOnEnter(TState state, Action action)
        {
            onEnterActions[state] = action;
        }

        public void SetOnExit(TState state, Action action)
        {
            onExitActions[state] = action;
        }

        public void UpdateState(bool backing)
        {
            if (!transitions.TryGetValue(CurrentState.Value, out Func<bool, TState?> transition)) return;

            TState? newState = transition.Invoke(backing);
            if (!newState.HasValue || EqualityComparer<TState>.Default.Equals(CurrentState.Value, newState.Value)) return;

            if (onExitActions.TryGetValue(CurrentState.Value, out Action onExit))
                onExit.Invoke();

            CurrentState.Value = newState.Value;

            if (onEnterActions.TryGetValue(CurrentState.Value, out Action onEnter))
                onEnter.Invoke();
        }
    }
}
