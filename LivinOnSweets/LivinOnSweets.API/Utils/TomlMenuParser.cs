using LivinOnSweets.API.Data;
using LivinOnSweets.API.Skinning;
using Tomlyn;
using Tomlyn.Syntax;

namespace LivinOnSweets.API.Utils
{
    public static class TomlMenuParser
    {
        // from resource pack manager
        private static readonly TomlModelOptions default_toml_options = new() { ConvertPropertyName = firstLowerPascal };

        public static MenuEntryInfo ParseToml(ResourcePackManager packManager, IResourcePack pack)
        {
            Stream entriesStream = pack.GetStream("MainMenu/menuEntries.toml");
            if (entriesStream == null)
                throw new ArgumentNullException();

            using StreamReader reader = new StreamReader(entriesStream);
            string tomlContent = reader.ReadToEnd();
            if (!Toml.TryToModel(tomlContent, out MenuEntryInfo menuEntryInfo,
                    out DiagnosticsBag diagnostics, options: default_toml_options))
            {
                throw new InvalidDataException();
            }

            if (menuEntryInfo.Inheritance.Enabled &&
                !string.IsNullOrEmpty(menuEntryInfo.Inheritance.Id))
                return applyInheritance(menuEntryInfo, packManager, pack);

            return menuEntryInfo;
        }

        private static MenuEntryInfo applyInheritance(MenuEntryInfo menuEntryInfo, ResourcePackManager packManager, IResourcePack pack)
        {
            // Should apply inheritance a little bit better but idk if this works properly lol
            // afaik apply missing entries/data to the entries it extends
            ResourcePack inheritPack = packManager.GetPackById(menuEntryInfo.Inheritance.Id);

            // will fallback into the safest option
            inheritPack ??= getSafestPack(packManager, pack);

            MenuEntryInfo inherited = ParseToml(packManager, inheritPack);
            foreach (MenuEntryInfo.EntryInfo entry in menuEntryInfo.Entries)
            {
                // we get the entry to extend or replace (mostly extend) and populate the missing fields (really no way on checking it better since we only care about the id bur uhh yea)
                MenuEntryInfo.EntryInfo extend = inherited.Entries.Find(e => string.Equals(e.Id, entry.Id, StringComparison.OrdinalIgnoreCase));
                if (extend != null)
                    entry.Populate(extend);
            }

            // should keep the order properly since this skips the already existing entries to add new ones but doesnt respect the order
            List<MenuEntryInfo.EntryInfo> missing = inherited.Entries
                .Where(p => !menuEntryInfo.Entries
                    .Any(c => string.Equals(c.Id, p.Id, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            menuEntryInfo.Entries.AddRange(missing);

            return menuEntryInfo;
        }

        private static ResourcePack getSafestPack(ResourcePackManager packManager, IResourcePack pack)
        {
            ResourcePack sugarRush = packManager.GetPackById(ResourcePackManager.OFFICIAL_RESOURCE_PACKS[0]);
            ResourcePack fallbackPack = packManager.GetPackById(pack.PackInfo.Metadata.Fallback);
            return fallbackPack ?? sugarRush;
        }

        private static string firstLowerPascal(string s)
        {
            char first = s[0]; // Take the first letter, e.g: M
            string rest = s[1..]; // Take the rest of the string except the first letter, e.g: etadata
            return char.ToLowerInvariant(first) + rest; // Join the stuff: metadata
        }
    }
}
