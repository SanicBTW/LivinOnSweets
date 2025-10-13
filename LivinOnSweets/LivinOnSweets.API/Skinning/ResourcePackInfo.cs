using System.Runtime.Serialization;
using JetBrains.Annotations;
using LivinOnSweets.ChartFormat.Adapters;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace LivinOnSweets.API.Skinning
{
    public class ResourcePackInfo
    {
        public MetadataTable Metadata { get; set; }
        public Dictionary<string, string> Aliases { get; set; } = [];
        public ScriptsTable Scripts { get; set; }
        public StoryTable Story { get; set; }
        public EngineTable Engine { get; set; }
        public SongsTable Songs { get; set; }

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

        public class StoryTable
        {
            public bool StoryProgression { get; set; } = true;
            public List<string> Chapters { get; set; } = [];
        }

        public class EngineTable
        {
            public List<string> CompatibleWith { get; set; } = [];
            public bool AllowHotReload { get; set; } = true;
            public bool RequiresRestart { get; set; } = false;
        }

        public class SongsTable
        {
            public string Format { get; set; } = DefaultAdapter.ChartFormatName;
            public List<string> Available { get; set; } = [];
            public bool MultiList { get; set; }

            [CanBeNull] public SongSeparatorTable Separator { get; set; }
        }

        public class SongSeparatorTable
        {
            public string LeftPart { get; set; } = "";
            public string RightPart { get; set; } = "";
            public string Background { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Label { get; set; } = ""; // should be set to label sprite since its used for the sprite lol!!
            public string Transition { get; set; } = "";
        }
    }
}
