using System.Globalization;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;
using Tomlyn;
using Tomlyn.Model;

namespace LivinOnSweets.API.Localisation
{
    // Uses Tomlyn to parse the localisation files
    public class TomlLocalisationStore : ILocalisationStore
    {
        private const char sub_separator = '.';

        private IResourceStore<byte[]> backingStore;

        private TomlTable backingToml;

        public CultureInfo EffectiveCulture { get; }

        public TomlLocalisationStore(IResourceStore<byte[]> store, string cultureCode)
        {
            backingStore = store;

            EffectiveCulture = new CultureInfo(cultureCode);
            populate();

            if (backingToml == null)
                throw new NullReferenceException($"Backing TOML is null, cannot use Localisation {cultureCode}");
        }

        public string Get(string lookup)
        {
            string[] split = lookup.Split(":");

            string section = split[0];
            string key = split[1];

            // Section content, pretty sure its a TomlTable
            TomlTable secCont = gettable(section);

            // Table Content, represents the value of the key
            if (!secCont.TryGetValue(key, out object tblCont))
                return null; // Fallback

            // Should stringify the table content if its not a string just in case, but we falling back for now
            if (tblCont is not string content)
                return null;

            return content;
        }

        public Task<string> GetAsync(string lookup, CancellationToken cancellationToken = default) => Task.FromResult(Get(lookup));

        public Stream GetStream(string name) => backingStore.GetStream(name);

        public IEnumerable<string> GetAvailableResources() => backingStore.GetAvailableResources();

        public void Dispose()
        {
            backingStore.Dispose();
        }

        private void populate()
        {
            string locale = EffectiveCulture.Name;

            using Stream stream = GetStream($"Localisation/{locale}.toml");
            if (stream == null) // Inside a try-catch
                throw new ArgumentNullException();

            using StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();

            backingToml = Toml.ToModel(content);
        }

        private TomlTable gettable(string section, TomlTable searchTable = null)
        {
            // If we dont have a search table use the backing one
            TomlTable table = searchTable ?? backingToml;

            int subIndex = section.IndexOf(sub_separator);

            if (subIndex == -1)
            {
                // No more separators return the final table
                return table.ContainsKey(section) ? table[section] as TomlTable : null;
            }

            // substring the section at the first separator
            string currentSection = section.Substring(0, subIndex);
            string remainingSections = section.Substring(subIndex + 1);

            // verify if the current section exists before going recursive
            if (!table.ContainsKey(currentSection) || !(table[currentSection] is TomlTable nextTable))
                return null;

            // recursive call for the next table section
            return gettable(remainingSections, nextTable);
        }
    }
}
