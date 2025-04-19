namespace LivinOnSweets.API.Configuration
{
    // Its cool to let the users play a different version from the mainstream one
    // Though it requires more work and cases to handle, its still a really cool customization option
    public enum GameUpdateVersion
    {
        // First release
        EventRelease,

        // Event finished, added vocals and changed the logo
        PostEvent,

        // The idol event
        AntiqueSeraphim,
    }
}
