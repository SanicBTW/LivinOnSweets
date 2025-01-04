using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinOnSweets.API.Stores
{
    // Store made to process accents from textures creating an image object with image sharp and processing it
    // TODO! Thread blocking operations, should be fixed sometime, only fix possible rn its to run the get calls under another thread, potentially making it asynchronous
    public class AccentStore(IResourceStore<byte[]> store) : ResourceStore<byte[]>(store)
    {
        public Colour4 GetDominantColor(string name, bool random = false) => GetDominantColors(name, 1, random).First();

        public Colour4[] GetDominantColors(string name, int amount = 3, bool random = false)
        {
            using Stream stream = GetStream(name);
            if (stream == null)
                return Enumerable.Repeat(Colour4.Black, amount).ToArray();

            // Not necessary to run Take since it already returns K colors (amount)
            return kMeans(extractPixels(stream), amount, random: random).Select(pixel => Rgba32ToColour4(pixel)).ToArray();
        }

        private List<Rgba32> extractPixels(Stream imgStream)
        {
            List<Rgba32> pixels = [];

            using Image<Rgba32> image = Image.Load<Rgba32>(imgStream);

            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                Rgba32 pixel = image[x, y];
                if (pixel.A <= 0) // skip fully transparent pixels
                    continue;

                pixels.Add(pixel);
            }

            return pixels;
        }

        private List<Rgba32> kMeans(List<Rgba32> pixels, int k, int maxIterations = 10, bool random = false)
        {
            List<Rgba32> centroids = initCentroids(pixels, k, random);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                List<List<Rgba32>> clusters = Enumerable.Range(0, k).Select(_ => new List<Rgba32>()).ToList();

                foreach (Rgba32 pixel in pixels)
                {
                    int closestCentroid = getClosestCentroid(pixel, centroids);
                    clusters[closestCentroid].Add(pixel);
                }

                List<Rgba32> newCentroids = clusters.Select(cluster =>
                {
                    if (cluster.Count == 0)
                    {
                        int randomIndex = RNG.Next(pixels.Count);
                        return pixels[randomIndex];
                    }

                    return new Rgba32(
                        (byte)cluster.Average(p => p.R),
                        (byte)cluster.Average(p => p.G),
                        (byte)cluster.Average(p => p.B),
                        255 // opaque color
                    );
                }).ToList();

                if (centroids.SequenceEqual(newCentroids))
                    break;

                centroids = newCentroids;
            }

            return centroids;
        }

        private List<Rgba32> initCentroids(List<Rgba32> pixels, int k, bool random)
        {
            List<Rgba32> centroids = [];

            if (random)
            {
                int randomIndex = RNG.Next(pixels.Count);
                centroids.Add(pixels[randomIndex]);
            }
            else
            {
                List<Rgba32> sortedPixels = pixels.OrderBy(pixel => pixel.R + pixel.G + pixel.B + pixel.A).ToList();

                int step = sortedPixels.Count / k;
                for (int i = 0; i < k; i++)
                {
                    centroids.Add(sortedPixels[i * step]);
                }
            }

            return centroids;
        }

        private int getClosestCentroid(Rgba32 pixel, List<Rgba32> centroids)
        {
            float minDist = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < centroids.Count; i++)
            {
                Rgba32 centroid = centroids[i];
                float distance = euclideanDistance(pixel, centroid);
                if (distance < minDist)
                {
                    minDist = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private float euclideanDistance(Rgba32 pixel, Rgba32 centroid) => MathF.Sqrt(
            MathF.Pow(pixel.R - centroid.R, 2) +
            MathF.Pow(pixel.G - centroid.G, 2) +
            MathF.Pow(pixel.B - centroid.B, 2)
        );

        public Colour4 Rgba32ToColour4(Rgba32 pixel) => new(pixel.R, pixel.G, pixel.B, pixel.A);
    }
}
