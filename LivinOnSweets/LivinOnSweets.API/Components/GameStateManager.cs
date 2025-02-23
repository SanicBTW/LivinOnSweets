using LivinOnSweets.API.Enums;
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
        public Bindable<RuntimeState> RtState = new();

        // Gameplay = GP
        public Bindable<GameplayState> GpState = new();

        // tries to guess the next runtime state it should be in
        public void UpdateRuntimeState(bool backing = false, bool checkGameplayState = false)
        {
            if (ProgressionBlock.Value || backing && !CanBack.Value)
                return;

            RuntimeState runtimeState = RtState.Value;
            GameplayState gameplayState = checkGameplayState ? GpState.Value : GameplayState.UNINITIALIZED;

            switch (runtimeState)
            {
                case RuntimeState.CLOSE_PROMPT:
                    // pressing back will close the prompt
                    // pressing confirm will close the game
                    RtState.Value = backing ? RuntimeState.STARTUP : RuntimeState.CLOSING;
                    break;

                case RuntimeState.STARTUP:
                    if (backing)
                    {
                        RtState.Value = gameplayState switch
                        {
                            GameplayState.UNINITIALIZED or GameplayState.INITIALIZED => RuntimeState.CLOSE_PROMPT,
                            _ => RtState.Value
                        };
                    }
                    else
                        RtState.Value = RuntimeState.IN_GAME;
                    break;

                case RuntimeState.IN_GAME:
                    // if its not backing then dont run the code below, since the game container will handle the
                    // states itself (Probably?)
                    if (!backing)
                        return;

                    RtState.Value = gameplayState switch
                    {
                        // Its in the main menu, not story mode, options or song selection
                        // sanco 1/5/25 - i noticed that the uninitialized state was missing in here, this was causing a blockage inside
                        // the game container preventing it from setting the rt state and blocking the exit from it if it failed to load
                        GameplayState.INITIALIZED or GameplayState.UNINITIALIZED => RuntimeState.STARTUP,
                        _ => RtState.Value
                    };
                    break;
            }
        }

        // Like the reset button on a console
        public void Reset()
        {
            CanBack.UnbindAll();
            ProgressionBlock.UnbindAll();
            RtState.UnbindAll();
            GpState.UnbindAll();

            CanBack.SetDefault();
            ProgressionBlock.SetDefault();
            RtState.SetDefault();
            GpState.SetDefault();
        }
    }
}
