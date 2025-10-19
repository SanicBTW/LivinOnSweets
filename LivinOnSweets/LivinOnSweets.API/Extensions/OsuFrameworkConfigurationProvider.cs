using AetherFramework;
using AetherFramework.Configuration;
using AetherFramework.Interfaces;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Extensions
{
    // not really an static extension but rather an extension over Aether Framework to support the storage object in the Aether config provider
    // based on the BasicConfig, the old OFConfigProvider code and IniConfigManager
    public class OsuFrameworkConfigurationProvider(Storage storage) : ConfigManager, IModConfigProvider
    {
        private static JsonSerializerSettings jsonSettings => new() { Formatting = Formatting.Indented };

        private string configFileName = "";
        private ModRegistry registry;
        private PresetConfigFile backerConfig = new();

        public string ProviderName => "osu!framework configuration provider";

        public void Setup(string configFile, ModRegistry modRegistry)
        {
            registry = modRegistry;
            configFileName = configFile;

            Load();
        }

        protected override void PerformLoad()
        {
            if (string.IsNullOrEmpty(configFileName)) return;

            using Stream fileStream = storage.GetStream(configFileName);
            if (fileStream == null) return;

            using StreamReader reader = new StreamReader(fileStream);
            string jsonRaw = reader.ReadToEnd();

            backerConfig = JsonConvert.DeserializeObject<PresetConfigFile>(jsonRaw) ?? new PresetConfigFile();

            registry.Refresh();
        }

        protected override bool PerformSave()
        {
            if (string.IsNullOrEmpty(configFileName)) return false;

            try
            {
                ((IModConfigProvider)this).Save();

                using Stream fileStream = storage.CreateFileSafely(configFileName);
                using StreamWriter writer = new StreamWriter(fileStream);

                string json = JsonConvert.SerializeObject(backerConfig, jsonSettings);
                writer.Write(json);
            }
            catch
            {
                return false;
            }

            return true;
        }

        // saves the values into the backer config from the registry
        void IModConfigProvider.Save()
        {
            backerConfig.EnabledMods = registry.GetEnabledMods().Select(mod => mod.Manifest.Name).ToList();
            backerConfig.DisabledMods = registry.GetDisabledMods().Select(mod => mod.Manifest.Name).ToList();
        }

        // i should revise the workflow or naming since sometimes (like this now) theres another method which
        // takes responsability over loading the config file, then calling this on the mod registry refresh
        // thus making the name misleading maybe since it only returns the backer config file, uhhh yeah
        ConfigFile IModConfigProvider.Load() => backerConfig;

    }
}
