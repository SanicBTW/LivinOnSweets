using LivinOnSweets.API.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;

namespace LivinOnSweets.API.Cursor
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Cursor/OsuContextMenuContainer.cs
    [Cached(typeof(SweetContextMenuContainer))]
    public partial class SweetContextMenuContainer : ContextMenuContainer
    {
        private SweetContextMenu menu;
        protected override Menu CreateMenu() => menu = new SweetContextMenu();
        public void CloseMenu() => menu.Close();
    }
}
