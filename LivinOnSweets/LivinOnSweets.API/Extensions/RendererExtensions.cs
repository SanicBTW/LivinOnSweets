using osu.Framework.Graphics.Rendering;
using osu.Framework.Threading;

namespace LivinOnSweets.API.Extensions
{
    // More extensions for the renderer
    public static class RendererExtensions
    {
        public static ScheduledDelegate WrapExpensiveOperation(this IRenderer renderer, Action task, double executionTime = 0D, double repeatInterval = -1D)
        {
            ScheduledDelegate operation = new(task, executionTime, repeatInterval);
            renderer.ScheduleExpensiveOperation(operation);
            return operation;
        }
    }
}
