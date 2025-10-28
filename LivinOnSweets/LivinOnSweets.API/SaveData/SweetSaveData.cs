using JetBrains.Annotations;
using LivinOnSweets.API.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Configuration;
using osu.Framework.Platform;

// this class was previously on configuration but since its not entirely related to it then im moving
namespace LivinOnSweets.API.SaveData
{
    // yet another file to track the save data NOT related to configuration
    // basically it will save the following: selected diff, menu entry, read chapters, unlocked songs, etc, all at stuff
    // its heavily based off the osu framework configuration provider for aether
    // also yeah, this can be easily modified to list out a different score or rank, etc, but thats pretty lame
    public class SweetSaveData : ConfigManager
    {
        private const string save_file = "v1_sweetsavedata.json";

        private static JsonSerializerSettings jsonSettings => new() { Formatting = Formatting.Indented, Converters = [new StringEnumConverter()] };

        private readonly Storage storage;
        private SaveDataInternal backerSave = new();
        private bool loadFail;

        public SweetSongDifficulty LastDifficulty
        {
            get => backerSave.LastDifficulty;
            set
            {
                if (backerSave.LastDifficulty == value)
                    return;

                backerSave.LastDifficulty = value;
                QueueBackgroundSave();
            }
        }

        public string LastMenuEntry
        {
            get => backerSave.LastMenuEntry;
            set
            {
                if (backerSave.LastMenuEntry == value)
                    return;

                backerSave.LastMenuEntry = value;
                QueueBackgroundSave();
            }
        }

        public int LastSongPlayed
        {
            get => backerSave.LastSongPlayed;
            set
            {
                if (backerSave.LastSongPlayed == value)
                    return;

                backerSave.LastSongPlayed = value;
                QueueBackgroundSave();
            }
        }

        public SweetSaveData(Storage storage)
        {
            this.storage = storage;

            Load();

            // this bs is based off the keybinds config in case you didnt notice
            if (!loadFail) return;

            // we really want to create the file soo uhh
            bool success = Save();
            if (!success) return; // we cant throw yet, instead we just give up

            loadFail = false;

            Load();
            if (loadFail)
                throw new Exception("Failed to load the save data from the new save file.");
        }

        protected override void PerformLoad()
        {
            if (string.IsNullOrEmpty(save_file))
            {
                loadFail = true;
                return;
            }

            using Stream fileStream = storage.GetStream(save_file);
            if (fileStream == null)
            {
                loadFail = true;
                return;
            }

            using StreamReader reader = new StreamReader(fileStream);
            string jsonRaw = reader.ReadToEnd();

            backerSave = JsonConvert.DeserializeObject<SaveDataInternal>(jsonRaw, jsonSettings);
        }

        protected override bool PerformSave()
        {
            if (string.IsNullOrEmpty(save_file)) return false;

            try
            {
                using Stream fileStream = storage.CreateFileSafely(save_file);
                using StreamWriter writer = new StreamWriter(fileStream);

                string json = JsonConvert.SerializeObject(backerSave, jsonSettings);
                writer.Write(json);
            }
            catch
            {
                return false;
            }

            return true;
        }

        // this should follow a guide or some template that indicates which chapter should be unlocked next OR if previous chapters were read
        public void MarkChapterAsUnlocked(string chapterId)
        {
            StorySave story = backerSave.Story;
            if (story.UnlockedChapters.Contains(chapterId))
                return;

            story.UnlockedChapters.Add(chapterId);
            QueueBackgroundSave();
        }

        public bool IsChapterUnlocked(string chapterId) => backerSave.Story.UnlockedChapters.Contains(chapterId);

        public void MarkChapterAsRead(string chapterId)
        {
            StorySave story = backerSave.Story;

            // checks if the chapter was unlocked previously before marking it as read
            if (!story.UnlockedChapters.Contains(chapterId) || story.ReadChapters.Contains(chapterId))
                return;

            story.ReadChapters.Add(chapterId);
            QueueBackgroundSave();
        }

        public bool IsChapterRead(string chapterId) => backerSave.Story.ReadChapters.Contains(chapterId);

        // song id should come in formatted already
        public void RecordSongStats(string songId, SongStats stats)
        {
            Dictionary<string, SongStats> songs = backerSave.SongStats;
            songs[songId] = stats; // will replace the existing one because uhh yeah
        }

        [CanBeNull] public SongStats GetSongStats(string songId) => backerSave.SongStats.GetValueOrDefault(songId);

        private class SaveDataInternal
        {
            public SweetSongDifficulty LastDifficulty { get; set; } = SweetSongDifficulty.Normal; // will populate an enum value rather than a string for good measure, this shouldnt apply to custom menus
            public string LastMenuEntry { get; set; } = "play"; // a string wthat will work as a lookup inside the menu population
            public int LastSongPlayed { get; set; } // an index too since the population order is always the same on song screens
            public StorySave Story { get; set; } = new(); // an object which contains: unlocks and reads
            public Dictionary<string, SongStats> SongStats { get; set; } = []; // an object which holds an object with index on diff and song that saves: score, judgements, max combo, rank, clearance stat (using flags),
        }
    }
}
