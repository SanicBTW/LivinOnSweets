using System.Reflection;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinOnSweets.API.Extensions
{
    // More extensions for the renderer
    public static class RendererExtensions
    {
        // Since the TakeScreenshot function is internal we'll have to use reflection to get the function
        private static MethodInfo takeScreenshotMethod = typeof(IRenderer).GetMethod("TakeScreenshot", BindingFlags.Instance | BindingFlags.NonPublic);

        public static ScheduledDelegate WrapExpensiveOperation(this IRenderer renderer, Action task, double executionTime = 0D, double repeatInterval = -1D)
        {
            ScheduledDelegate operation = new(task, executionTime, repeatInterval);
            renderer.ScheduleExpensiveOperation(operation);
            return operation;
        }

        public static Image<Rgba32> TakeScreenshotToImage(this IRenderer renderer) =>
            (Image<Rgba32>)takeScreenshotMethod.Invoke(renderer, null);

        public static Texture TakeScreenshotToTexture(this IRenderer renderer)
        {
            Image<Rgba32> image = renderer.TakeScreenshotToImage();

            Texture texture = renderer.CreateTexture(image.Width, image.Height);
            texture.SetData(new TextureUpload(image));

            return texture;
        }
    }
}
