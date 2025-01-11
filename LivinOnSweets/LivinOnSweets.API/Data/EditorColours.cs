using LivinOnSweets.API.Sprites;
using LivinOnSweets.API.Stores;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace LivinOnSweets.API.Data
{
    public static class EditorColours
    {
        // Left Banner Accent Colors
        public static BindableList<Colour4> PrimaryColors = new();

        // Right Banner Accent Colors
        public static BindableList<Colour4> SecondaryColors = new();

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
