using LivinOnSweets.API.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Configuration;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Configuration
{
    // This code only works as a configuration manager for the keybinds saved on a JSON, doesn't contain any input handling by itself
    // Coming from FunkinSharp Gen 1 https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Funkin/FunkinKeybinds.cs

    // TODO: Properly implement defaults instead of throwing exceptions?
    public class KeybindsConfig : ConfigManager<ManiaAction>
    {
        #if DEBUG
        public const string FILENAME = "v2x_Keybinds.json";
        #else
        public const string FILENAME = "v2_Keybinds.json";
        #endif

        private readonly Storage storage;

        protected readonly IDictionary<ManiaAction, object> DefaultOverrides;

        // setting it to null will throw an exception if it didnt load correctly, we keeping it with this behaviour just to avoid any edge case
        public IEnumerable<IKeyBinding> LoadedKeybinds;

        protected bool LoadError; // add edge cases for first load

        public KeybindsConfig(Storage storage, IDictionary<ManiaAction, object> defaultOverrides = null)
            : base(defaultOverrides)
        {
            this.storage = storage;
            DefaultOverrides = defaultOverrides;

            Load();

            if (!LoadError) return;

            // if the save was successful, we load the values into the Keybindings enumerable
            // to save up time and avoid parsing more on the container
            bool success = Save();

            if (!success)
                throw new Exception("Failed to save the default keybinds.");

            LoadError = false;

            // you might think this is unnecessary and... it is! but might save us from some places so
            Load();
            if (LoadError)
                throw new Exception("Failed to load the keybinds from the new configuration file");
        }

        protected override void PerformLoad()
        {
            // if not found, call save to create the file
            if (!storage.Exists(FILENAME))
            {
                LoadError = true;
                return;
            }

            try
            {
                // we using filemode open since we already checking if the file exists
                using Stream stream = storage.GetStream(FILENAME, FileAccess.Read, FileMode.Open);
                using StreamReader sr = new StreamReader(stream);

                ActionEntry[] keys = JsonConvert.DeserializeObject<JsonStruct>(sr.ReadToEnd()).Keys;
                List<IKeyBinding> loadKeybinds = [];

                foreach (ActionEntry entry in keys)
                {
                    if (entry.IsCombination)
                        loadKeybinds.Add(new KeyBinding(new KeyCombination(entry.Keys), entry.Action));
                    else
                    {
                        // we creating a new KeyBinding object for each key that exists for the action
                        loadKeybinds.AddRange(entry.Keys.Select(key => new KeyBinding(key, entry.Action)));
                    }
                }

                LoadedKeybinds = loadKeybinds;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to parse the keybinds file.");
            }
        }

        // should probably decouple this whole method but i doubt anyone will mess with this so imma keep going
        protected override bool PerformSave()
        {
            try
            {
                using Stream stream = storage.CreateFileSafely(FILENAME);
                using StreamWriter sw = new StreamWriter(stream);

                JsonSerializerSettings settings = new();
                // To save the value as the enum name rather than the enum index
                settings.Converters.Add(new StringEnumConverter());
                settings.Formatting = Formatting.Indented;

                List<ActionEntry> temp = [];
                if (LoadedKeybinds != null)
                {
                    // REVISE THIS BEHAVIOUR, I HAD A DUMB CAST BEFORE
                    // 12/1 sanco: added this thing just in case this goes under the hood, i cant really check this rn so ill have to check this on a later point of the development
                    throw new NotImplementedException();
                    foreach (IKeyBinding kb in LoadedKeybinds)
                    {
                        temp.Add(new ActionEntry((ManiaAction)kb.Action, [.. kb.KeyCombination.Keys], kb.KeyCombination.Keys.Length > 1));
                    }
                }
                else
                {
                    // default overrides CANT be null here
                    // Thanks JetBrains Rider
                    temp.AddRange(
                        from defEntry in DefaultOverrides
                        let value = (ManiaActionContainer.KeyDict)defEntry.Value
                        select new ActionEntry(defEntry.Key, [.. value.Keys], value.IsCombination)
                    );
                }

                ActionEntry[] keys = [.. temp];
                JsonStruct json = new(keys);
                sw.Write(JsonConvert.SerializeObject(json, settings));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save the keybinds file.");
            }

            return false;
        }

        public bool ShowOnExplorer() => storage.PresentFileExternally(FILENAME);

        private readonly struct JsonStruct(ActionEntry[] keys)
        {
            [JsonProperty("keybinds")]
            public readonly ActionEntry[] Keys = keys;
        }

        private readonly struct ActionEntry(ManiaAction action, InputKey[] keys, bool isCombination)
        {
            [JsonProperty("action")]
            public readonly ManiaAction Action = action;

            [JsonProperty("keys")]
            public readonly InputKey[] Keys = keys;

            [JsonProperty("isCombination")]
            public readonly bool IsCombination = isCombination;
        }
    }
}
