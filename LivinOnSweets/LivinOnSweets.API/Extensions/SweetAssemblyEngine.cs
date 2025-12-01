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
        private static readonly IAetherLogger logger;

        static SweetAssemblyEngine()
        {
            logger = AetherLog.CreateScoped($"{nameof(SweetAssemblyEngine)}");

            AssemblyLoadContext.Default.Resolving += resolveFromAppDomain;
        }

        protected override void LoadMods(string path, string filePrefix)
        {
            // its most likely to be only added ONCE, also we remove the first resolve to prioritize storage
            AssemblyLoadContext.Default.Resolving -= resolveFromAppDomain;
            AssemblyLoadContext.Default.Resolving += resolveFromStorage;
            AssemblyLoadContext.Default.Resolving += resolveFromAppDomain;

            bool shouldCheckPrefix = !string.IsNullOrEmpty(filePrefix);

            // since we are already inside the mods folder, we dont need to pass the given path
            string filter = $"{filePrefix}*.dll";
            string[] mods = storage.GetFiles("", filter).ToArray();
            logger.Info($"Found {mods.Length} possible mods matching {filter}");

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

        private Assembly resolveFromStorage(AssemblyLoadContext loadContext, AssemblyName name)
        {
            logger.Info($"Load Context \"{loadContext.Name}\" failed to resolve assemblies by itself, resolving assembly {name.Name} from the Storage ({storage.GetFullPath("")})");

            string filter = $"{name.Name}.dll";
            string[] matchinAssemblies = storage.GetFiles("", filter).ToArray();
            logger.Info($"Found {matchinAssemblies.Length} assemblies with {filter}");

            if (matchinAssemblies.Length == 0)
            {
                logger.Error($"Failed to load assembly {name.Name} from the Storage.");
                return null;
            }

            Stream assemblyStream = storage.GetStream(matchinAssemblies[0]);
            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            logger.Info($"Retrieved assembly {name.Name} from the Storage.");

            return assembly;
        }

        private static Assembly resolveFromAppDomain(AssemblyLoadContext loadContext, AssemblyName name)
        {
            logger.Info($"Load Context \"{loadContext.Name}\" failed to resolve assemblies by itself, resolving assembly {name.Name} from the AppDomain.");

            AppDomain appDomain = AppDomain.CurrentDomain;
            Assembly assembly = appDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name!.Equals(name.Name, StringComparison.OrdinalIgnoreCase));
            if (assembly != null)
                logger.Info($"Retrieved assembly {name.Name} from the AppDomain.");
            else
                logger.Error($"Failed to load assembly {name.Name} from the AppDomain.");

            return assembly;
        }
    }
}
