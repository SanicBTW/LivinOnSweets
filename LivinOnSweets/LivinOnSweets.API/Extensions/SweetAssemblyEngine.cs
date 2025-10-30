using System.Reflection;
using System.Runtime.Loader;
using AetherFramework;
using AetherFramework.Engines;
using AetherFramework.Interfaces;
using JetBrains.Annotations;
using osu.Framework.Platform;

namespace LivinOnSweets.API.Extensions
{
    // yeahh uhh i should really stop naming things with sweet bruh
    // basically this assembly engine works with a storage element which is useful to avoid writing duplicate code when the sweet android storage already handles streams and stuff inside the SAF folder
    public class SweetAssemblyEngine(Storage storage, [CanBeNull] IModConfigProvider config = null) : BaseModEngine("aether_assembly_config.json", config)
    {
        protected override void LoadMods(string path, string filePrefix)
        {
            bool shouldCheckPrefix = !string.IsNullOrEmpty(filePrefix);

            // since we are already inside the mods folder, we dont need to pass the given path
            string[] mods = storage.GetFiles("", $"{filePrefix}*.dll").ToArray();

            foreach (string modFile in mods)
            {
                if (shouldCheckPrefix && !modFile.StartsWith(filePrefix))
                    continue;

                Stream assemblyStream = storage.GetStream(modFile);
                Assembly modAssembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
                ClassRegistry.RegisterOverridableClasses(modAssembly);
                IEnumerable<Type> modTypes = modAssembly.GetTypes()
                    .Where(t => typeof(IMod).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

                foreach (Type type in modTypes)
                {
                    IMod mod = (IMod)Activator.CreateInstance(type)!;
                    Registry.RegisterMod(mod);
                }
            }
        }
    }
}
