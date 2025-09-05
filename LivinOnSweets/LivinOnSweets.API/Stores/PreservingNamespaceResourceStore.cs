using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // https://github.com/ppy/osu-framework/blob/master/osu.Framework/IO/Stores/NamespacedResourceStore.cs
    // Behaves like a NamespacedResourceStore but PRESERVES the namespace in the paths
    // You will ask me, "then why not just override the methods?" because when overriding something with "new" it doesnt override the inheritance calls afaik
    // So it would still behave with the old methods, not ideal
    public class PreservingNamespaceResourceStore<T>(IResourceStore<T> store, string ns) : ResourceStore<T>(store)
        where T : class
    {
        // custom filenames implementation since the possible paths it might use can start with the namespace itself, if thats the case dont make a string, just use the name
        protected override IEnumerable<string> GetFilenames(string name) => name.StartsWith(ns) ? base.GetFilenames(name) : base.GetFilenames($"{ns}/{name}");

        public override IEnumerable<string> GetAvailableResources() => base.GetAvailableResources()
                                                                            .Where(x => x.StartsWith($"{ns}/", StringComparison.Ordinal));
    }
}
