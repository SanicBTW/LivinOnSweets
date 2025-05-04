using System.Runtime.Serialization;
using JetBrains.Annotations;
// ReSharper disable ClassNeverInstantiated.Global

namespace LivinOnSweets.API.Skinning
{
    public class ResourcePackInfo
    {
        public MetadataTable Metadata { get; set; }
        public Dictionary<string, string> Aliases { get; set; } = [];
        public ScriptsTable Scripts { get; set; }
        public FeaturesTable Features { get; set; }
        public EngineTable Engine { get; set; }

        public class MetadataTable
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Author { get; set; } = "";

            [CanBeNull] public string Fallback { get; set; } = null;

            public string Version { get; set; }

            [IgnoreDataMember]
            public Version SerializedVersion => new(Version);
        }

        public class ScriptsTable
        {
            public bool Enabled { get; set; } = false;
            public string Loader { get; set; } = "";
            public string SearchFolder { get; set; } = "";
            public bool Reloadable { get; set; } = true;
        }

        public class FeaturesTable
        {
            public bool StoryModeProgression { get; set; } = true;
        }

        public class EngineTable
        {
            // ReSharper disable once CollectionNeverUpdated.Global
            public List<string> CompatibleWith { get; set; } = [];
            public bool AllowHotReload { get; set; } = true;
            public bool RequiresRestart { get; set; } = false;
        }
    }
}
