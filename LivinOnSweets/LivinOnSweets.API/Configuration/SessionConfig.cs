using osu.Framework;
using osu.Framework.Configuration;

namespace LivinOnSweets.API.Configuration
{
    // Similar to SessionStatics from lazer, this class only modifies the perform functions, just like InMemoryConfigManager
    public class SessionConfig : ConfigManager<SessionSetting>
    {
        public SessionConfig()
        {
            InitialiseDefaults();
        }

        protected override void InitialiseDefaults()
        {
            SetDefault(SessionSetting.TouchInputActive, RuntimeInfo.IsMobile);
            SetDefault(SessionSetting.ShowingScreenshot, false);
        }

        protected override void PerformLoad() { }

        protected override bool PerformSave() => true;
    }
}
