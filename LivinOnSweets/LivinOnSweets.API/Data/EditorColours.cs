using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace LivinOnSweets.API.Data
{
    // Kinda lame I know lol, I shouldn't use static bs
    public static class EditorColours
    {
        // Left Banner Accent Colors
        public static BindableList<Colour4> PrimaryColors = [];

        // Right Banner Accent Colors
        public static BindableList<Colour4> SecondaryColors = [];

        public static void Reset()
        {
            PrimaryColors.Clear();
            SecondaryColors.Clear();
        }

        public static void PopulateColors(AccentStore accentStore, StudentBanner banner, BindableList<Colour4> target)
        {
            Task.Run(() =>
            {
                Colour4[] accents = accentStore.GetDominantColors(banner.ImageName);
                target.AddRange(accents);
            });
        }
    }
}
