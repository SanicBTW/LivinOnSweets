using LivinOnSweets.API.StateMachines;
using osu.Framework.Bindables;

namespace LivinOnSweets.API.Components
{
    // although this should be inside StateMachines/ I thought this was a more appropiate place since it handles the
    // logic alltogether, however the state machine class handles the logic of state machines, not the game itself
    // this is a rewrite and port from the old code: https://github.com/SanicBTW/LivinOnSweets/blob/master/LivinOnSweets/LivinOnSweets.API/Components/GameStateManager.cs
    // not the latest one but its kinda whats based off
    public class GameStateManager
    {
        public BindableBool CanBack = new(true);
        public BindableBool ProgressionBlock = new();

        public StateMachine<RuntimeState> RuntimeMachine = new();
        public StateMachine<GameplayState> GameplayMachine = new();

        public GameStateManager()
        {
            RuntimeMachine.CurrentState.Value = RuntimeState.Startup;
            setupRuntimeTransitions();

            GameplayMachine.CurrentState.Value = GameplayState.NotReady;
            setupGameplayTransitions();
        }

        private void setupRuntimeTransitions()
        {
            RuntimeMachine.SetTransition(RuntimeState.ClosePrompt, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                return backing ? RuntimeState.Startup : RuntimeState.Closing;
            });

            RuntimeMachine.SetTransition(RuntimeState.Startup, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                GameplayState gameplayState = GameplayMachine.CurrentState.Value;

                if (backing)
                {
                    if (gameplayState <= GameplayState.Ready) // encapsulates NotReady (0) and maxes to Ready (1)
                        return RuntimeState.ClosePrompt;

                    return null;
                }
                else
                    return RuntimeState.InGame;
            });

            RuntimeMachine.SetTransition(RuntimeState.InGame, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                // if its not backing then dont run the code below, since the game container will handle the
                // states itself (Probably?)
                if (!backing)
                    return null;

                GameplayState gameplayState = GameplayMachine.CurrentState.Value;
                if (gameplayState <= GameplayState.Ready) // same as above lol
                    return RuntimeState.Startup;

                return null;
            });
        }

        private void setupGameplayTransitions()
        {
            GameplayMachine.SetTransition(GameplayState.NotReady, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                return backing ? null : GameplayState.Ready;
            });

            GameplayMachine.SetTransition(GameplayState.PlayingSong, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                return backing ? GameplayState.SongPaused : null;
            });

            GameplayMachine.SetTransition(GameplayState.SongPaused, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                return backing ? GameplayState.SongSelect : null;
            });

            GameplayMachine.SetTransition(GameplayState.ReadingChapter, backing =>
            {
                if (ProgressionBlock.Value || backing && !CanBack.Value)
                    return null;

                return backing ? GameplayState.StorySelect : null;
            });

            GameplayMachine.SetTransition(GameplayState.SongSelect, (backing) => goToMainMenu(backing, GameplayState.PlayingSong));
            GameplayMachine.SetTransition(GameplayState.StorySelect, (backing) => goToMainMenu(backing, GameplayState.ReadingChapter));

            GameplayMachine.SetTransition(GameplayState.GameOptions, (backing) => goToMainMenu(backing));
        }

        private GameplayState? goToMainMenu(bool backing, GameplayState? nextState = null)
        {
            if (ProgressionBlock.Value || backing && !CanBack.Value)
                return null;

            return backing ? GameplayState.Ready : nextState;
        }

        public void UpdateStates(bool backing)
        {
            RuntimeMachine.UpdateState(backing);
            GameplayMachine.UpdateState(backing);
        }

        public void Reset()
        {
            CanBack.UnbindAll();
            ProgressionBlock.UnbindAll();
            RuntimeMachine.CurrentState.UnbindAll();
            GameplayMachine.CurrentState.UnbindAll();

            CanBack.SetDefault();
            ProgressionBlock.SetDefault();
            RuntimeMachine.CurrentState.SetDefault();
            GameplayMachine.CurrentState.SetDefault();
        }
    }
}
