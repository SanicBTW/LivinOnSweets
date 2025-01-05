using LivinOnSweets.API.Enum;
using osu.Framework.Bindables;

namespace LivinOnSweets.API.Components
{
    // Basic class that stores states across screens, populated through the dependency container for easier access
    public class GameStateManager
    {
        // Bindable to control if updating the states backwards is possible
        public BindableBool CanBack = new(true);

        // Bindable to control if it can update any state at all
        public BindableBool ProgressionBlock = new();

        // Runtime = RT
        public Bindable<RuntimeState> RTState = new Bindable<RuntimeState>();

        // Gameplay = GP
        public Bindable<GameplayState> GPState = new Bindable<GameplayState>();

        // tries to guess the next state it should be in
        public void UpdateRuntimeState(bool backing = false, bool checkGameplayState = false)
        {
            if (ProgressionBlock.Value || backing && !CanBack.Value)
                return;

            RuntimeState runtimeState = RTState.Value;
            GameplayState gameplayState = checkGameplayState ? GPState.Value : GameplayState.UNINITIALIZED;

            switch (runtimeState)
            {
                case RuntimeState.CLOSE_PROMPT:
                    // pressing back will close the prompt
                    // pressing confirm will close the game
                    // RTState.Value = backing ? RuntimeState.STARTUP : RuntimeState.CLOSING;
                    RTState.Value = RuntimeState.STARTUP;
                    break;

                case RuntimeState.STARTUP:
                    if (backing)
                    {
                        RTState.Value = gameplayState switch
                        {
                            GameplayState.UNINITIALIZED or GameplayState.INITIALIZED => RuntimeState.CLOSE_PROMPT,
                            _ => RTState.Value
                        };
                    }
                    else
                        RTState.Value = RuntimeState.IN_GAME;
                    break;

                case RuntimeState.IN_GAME:
                    // if its not backing then dont run the code below, since the game container will handle the
                    // states itself (Probably?)
                    if (!backing)
                        return;

                    RTState.Value = gameplayState switch
                    {
                        // Its in the main menu, not story mode, options or song selection
                        // sanco 1/5/25 - i noticed that the uninitialized state was missing in here, this was causing a blockage inside
                        // the game container preventing it from setting the rt state and blocking the exit from it if it failed to load
                        GameplayState.INITIALIZED or GameplayState.UNINITIALIZED => RuntimeState.STARTUP,
                        _ => RTState.Value
                    };
                    break;
            }
        }

        // Like the reset button on a console
        public void Reset()
        {
            CanBack.UnbindAll();
            ProgressionBlock.UnbindAll();
            RTState.UnbindAll();
            GPState.UnbindAll();

            CanBack.SetDefault();
            ProgressionBlock.SetDefault();
            RTState.SetDefault();
            GPState.SetDefault();
        }
    }
}
