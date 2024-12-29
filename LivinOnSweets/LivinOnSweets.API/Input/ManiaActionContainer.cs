using LivinOnSweets.API.Configuration;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Input
{
    // This class is added on top of the tree hierarchy, in order to pump down key presses effectively
    public partial class ManiaActionContainer : KeyBindingContainer<ManiaAction>
    {
        protected KeybindsConfig KBConfig;
        protected DependencyContainer GameDependencies;

        public override IEnumerable<IKeyBinding> DefaultKeyBindings =>
        [
            // GLOBAL

            new KeyBinding(InputKey.Enter, ManiaAction.CONFIRM),

            new KeyBinding(InputKey.Escape, ManiaAction.BACK),

            new KeyBinding(new KeyCombination([InputKey.LShift, InputKey.F5]), ManiaAction.REFRESH),

            // VOLUME

            new KeyBinding(InputKey.KeypadPlus, ManiaAction.VOLUME_UP),
            new KeyBinding(InputKey.BracketRight, ManiaAction.VOLUME_UP), // usually the key next to the largest part of the enter key ig

            new KeyBinding(InputKey.KeypadMinus, ManiaAction.VOLUME_DOWN),
            new KeyBinding(InputKey.Slash, ManiaAction.VOLUME_DOWN), // usually the key next to the right shift key

            new KeyBinding(InputKey.Keypad0, ManiaAction.VOLUME_MUTE),
            new KeyBinding(InputKey.Number0, ManiaAction.VOLUME_MUTE),

            // UI

            new KeyBinding(InputKey.Left, ManiaAction.UI_LEFT),

            new KeyBinding(InputKey.Down, ManiaAction.UI_DOWN),

            new KeyBinding(InputKey.Up, ManiaAction.UI_UP),

            new KeyBinding(InputKey.Right, ManiaAction.UI_RIGHT),

            // NOTE

            new KeyBinding(InputKey.D, ManiaAction.NOTE_LEFT),
            new KeyBinding(InputKey.Left, ManiaAction.NOTE_LEFT),

            new KeyBinding(InputKey.F, ManiaAction.NOTE_DOWN),
            new KeyBinding(InputKey.Down, ManiaAction.NOTE_DOWN),

            new KeyBinding(InputKey.J, ManiaAction.NOTE_UP),
            new KeyBinding(InputKey.Up, ManiaAction.NOTE_UP),

            new KeyBinding(InputKey.K, ManiaAction.NOTE_RIGHT),
            new KeyBinding(InputKey.Right, ManiaAction.NOTE_RIGHT),
        ];

        [BackgroundDependencyLoader]
        private void load(Storage userStorage)
        {
            IDictionary<ManiaAction, object> converted = ConvertDefaults();

            // We add the configuration to the dependencies so it can be retrieved thru BDL in child containers
            GameDependencies.CacheAs(KBConfig = new KeybindsConfig(userStorage, converted));
        }

        protected override void ReloadMappings()
        {
            // if there are no keybinds loaded, call the base method to load up the default keybinds
            // 23/12/24 (3:43 am) sanco: but probably this wont reach since theres a ton of exceptions in the way so uhh just in case??
            if (KBConfig.LoadedKeybinds == null)
            {
                base.ReloadMappings();
                return;
            }

            KeyBindings = KBConfig.LoadedKeybinds;
        }

        protected virtual IDictionary<ManiaAction, object> ConvertDefaults()
        {
            List<IKeyBinding> kbs = DefaultKeyBindings.ToList();
            Dictionary<ManiaAction, object> dict = [];

            // TODO: Finish documenting the whole process of conversion lol
            // We use the length of kb keycomb keys because it REALLY defines if its a combination, more than 2 keys are needed for one, so, if its only one then its probably some alt keybinds lol
            foreach (IKeyBinding kb in kbs)
            {
                ManiaAction action = (ManiaAction)kb.Action;

                if (!dict.TryGetValue(action, out object value))
                {
                    InputKey[] keys = [.. kb.KeyCombination.Keys];
                    value = new KeyDict([.. keys], keys.Length > 1);
                }

                // casting and spread operator yessir we love that here
                KeyDict castedSave = (KeyDict)value;
                List<InputKey> castedVal = CheckKey(castedSave.Keys, kb.KeyCombination);

                dict[action] = new KeyDict(castedVal, kb.KeyCombination.Keys.Length > 1);
            }

            return dict;
        }

        protected virtual List<InputKey> CheckKey(List<InputKey> defaultValue, KeyCombination kb)
        {
            List<InputKey> castedVal = defaultValue;

            foreach (InputKey key in kb.Keys)
            {
                if (!castedVal.Contains(key))
                    castedVal.Add(key);
            }

            return castedVal;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new(base.CreateChildDependencies(parent));

        internal readonly struct KeyDict(List<InputKey> keys, bool isCombination)
        {
            public readonly List<InputKey> Keys = keys;
            public readonly bool IsCombination = isCombination;
        }
    }
}
