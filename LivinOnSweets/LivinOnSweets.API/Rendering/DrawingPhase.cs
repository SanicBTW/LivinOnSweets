namespace LivinOnSweets.API.Rendering
{
    /// <summary>
    /// Simple enum representing the state of drawing where the drawing will get altered.
    /// </summary>
    public enum DrawingPhase
    {
        /// <summary>
        /// Draws before the DrawNode Draw call.
        /// </summary>
        PreDraw,

        /// <summary>
        /// Draws after the DrawNode Draw call.
        /// </summary>
        PostDraw,
    }
}
