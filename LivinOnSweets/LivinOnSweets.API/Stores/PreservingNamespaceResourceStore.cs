using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    // https://github.com/ppy/osu-framework/blob/1022955ac5529357b8c29b4d9f54fe4d99ed9b68/osu.Framework/IO/Stores/NamespacedResourceStore.cs
    // Behaves like a NamespacedResourceStore but PRESERVES the namespace in the paths
    // You will ask me, "then why not just override the methods?" because when overriding something with "new" it doesnt override the inheritance calls afaik
    // So it would still behave with the old methods, not ideal
    public class PreservingNamespaceResourceStore<T>(IResourceStore<T> store, string ns) : ResourceStore<T>(store)
        where T : class
    {
        public string Namespace = ns;

        // custom filenames implementation since the possible paths it might use can start with the namespace itself, if thats the case dont make a string, just use the name
        protected override IEnumerable<string> GetFilenames(string name) => name.StartsWith(Namespace) ? base.GetFilenames(name) : base.GetFilenames($@"{Namespace}/{name}");

        public override IEnumerable<string> GetAvailableResources() => base.GetAvailableResources()
                                                                            .Where(x => x.StartsWith($"{Namespace}/", StringComparison.Ordinal));
    }
}
