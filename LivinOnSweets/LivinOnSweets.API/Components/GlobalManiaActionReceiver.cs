using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Input;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Components
{
    // Component which handles the global keypresses, since the game container isnt under ManiaActionContainer, we add this component to leverage the work
    public partial class GlobalManiaActionReceiver : Component, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private SweetConfigManager sweetConfig { get; set; }

        private BindableBool showFpsDisplay;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showFpsDisplay = (BindableBool)sweetConfig.GetBindable<bool>(SweetSetting.ShowFpsDisplay);
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            switch (e.Action)
            {
                case ManiaAction.VOLUME_UP:
                case ManiaAction.VOLUME_DOWN:
                case ManiaAction.VOLUME_MUTE:
                    return true;
            }

            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case ManiaAction.TOGGLE_FPS:
                    // We can't access the FPS Counter here (its exposed within Game) so we just set the settings variable
                    showFpsDisplay.Toggle();
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }
    }

}
