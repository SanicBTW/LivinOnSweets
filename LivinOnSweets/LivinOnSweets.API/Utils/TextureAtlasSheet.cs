using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

// this obviously needs more work like for example holding cached frames and more to be able to reuse them properly
namespace LivinOnSweets.API.Utils
{
    /// <summary>
    /// Helper class to ease up the creation of <see cref="TextureAtlas"/>es for animated spritesheets without allocating the huge <see cref="Texture"/> of the spritesheet.
    ///
    /// This is to reduce the memory usage for creating mipmaps or just allocating the texture in the VRAM.
    /// </summary>
    public static class TextureAtlasSheet
    {
        /// <summary>
        /// The maximum <see cref="TextureAtlas"/> size on each axis.
        /// </summary>
        public const int MAX_ATLAS_SIZE = 1024;

        /// <summary>
        /// The maximum size of each frame of the animation.
        /// </summary>
        public const int MAX_FRAME_SIZE = 128;

        private static readonly FieldInfo image_texture_upload =
            typeof(TextureUpload).GetField("image", BindingFlags.Instance | BindingFlags.NonPublic);

        // my previous algorithm to guess the amount of atlases to use is redundant, since when an atlas is overflowing
        // it will create another one changing the destination of the atlas, I dont know if its preffered to allocate
        // the needed atlases rather than letting the atlas fallback to another one in the middle of additions
        /// <summary>
        /// Retrieves a <see cref="TextureAtlas"/> and the spritesheet <see cref="Image{TPixel}"/> to get the frames of the sheet through <see cref="CropAndResize"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IAtlasSheetProvider"/> to use, acting as the origin.</param>
        /// <param name="sheetPath">The image path of the spritesheet.</param>
        /// <param name="manualMipmaps">If the <see cref="TextureAtlas"/> shouldn't make mipmapping automatically.</param>
        /// <param name="filterMode">The filter of the returned <see cref="TextureAtlas"/>.</param>
        /// <returns>A fresh <see cref="Image{TPixel}"/> and <see cref="TextureAtlas"/> for usage, or null if the initial <see cref="Stream"/> lookup for <paramref name="sheetPath"/> fails.</returns>
        public static (Image<Rgba32>, TextureAtlas) RetrieveAtlasAndImage(this IAtlasSheetProvider provider, string sheetPath, bool manualMipmaps = false, TextureFilteringMode filterMode = TextureFilteringMode.Linear)
        {
            Stream sheetStream = provider.GetStream(sheetPath);
            if (sheetStream == null)
            {
                // should warn about a failed lookup
                return (null, null);
            }

            TextureUpload sheetUpload = new TextureUpload(sheetStream);

            // we should guard against this value being null but whatever
            Image<Rgba32> sheetImage = (Image<Rgba32>)image_texture_upload.GetValue(sheetUpload);

            // we dont let the user change this size since the opengl renderer caps it to 1024x1024
            TextureAtlas sheetAtlas =
                new TextureAtlas(provider.Renderer, MAX_ATLAS_SIZE, MAX_ATLAS_SIZE, manualMipmaps, filterMode);

            return (sheetImage, sheetAtlas);
        }

        /// <summary>
        /// Crops the target <paramref name="image"/> to get the frame and resizes it to fit inside the <see cref="TextureAtlas"/>.
        /// </summary>
        /// <param name="image">The target <see cref="Image{TPixel}"/> to crop and resize.</param>
        /// <param name="position">The position of the frame.</param>
        /// <param name="size">The size of the frame.</param>
        /// <param name="frameSize">The frame size for the <see cref="TextureAtlas"/>, defaults to <see cref="MAX_FRAME_SIZE"/>, manipulating this could lead to the usage of more <see cref="TextureAtlas"/>es.</param>
        /// <param name="sampler">The <see cref="IResampler"/> to process the resize with.</param>
        /// <returns>A fresh frame of the target <paramref name="image"/>.</returns>
        public static Image<Rgba32> CropAndResize(this Image<Rgba32> image, Vector2I position, Vector2I size, Vector2I? frameSize = null, [CanBeNull] IResampler sampler = null)
        {
            IResampler finalSampler = sampler ?? new NearestNeighborResampler();
            Vector2I finalFrameSize = frameSize ?? new Vector2I(MAX_FRAME_SIZE);

            Image<Rgba32> frameImage = image.Clone(ctx => ctx.Crop(new Rectangle(position.X, position.Y, size.X, size.Y)));
            frameImage.Mutate(ctx => ctx.Resize(finalFrameSize.X, finalFrameSize.Y, finalSampler));
            return frameImage;
        }
    }

    public interface IAtlasSheetProvider : IFramedAnimation
    {
        IRenderer Renderer { get; }
        Stream GetStream(string sheetPath);
    }
}
