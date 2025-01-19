using System.Collections.Concurrent;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LivinOnSweets.API.Stores
{
    // Store made to process accents from textures creating an image object with image sharp and processing it
    // Just noticed that when doing an amount of 1, the provided color doesnt match the accents given by an amount of 3, this is probably caused by centroids being sensitive to the amount of clusters provided, should be fixed by now
    // sanco here, a day later, uhhh i dont believe it improved that much nor fixed the original issue but whatever
    // TODO! Thread blocking operations, should be fixed sometime, only fix possible rn its to run the get calls under another thread, potentially making it asynchronous (please look at the tests)
    // TODO! Check the new caching feature
    public class AccentStore(IResourceStore<byte[]> store) : ResourceStore<byte[]>(store)
    {
        private ConcurrentDictionary<string, Colour4[]> accentCache = new();

        public Colour4 GetDominantColor(string name, bool random = false) => GetDominantColors(name, 1, random).First();

        public Colour4[] GetDominantColors(string name, int amount = 3, bool random = false)
        {
            using Stream stream = GetStream(name);
            if (stream == null)
                return Enumerable.Repeat(Colour4.Black, amount).ToArray();

            bool skipKMeans = amount == 1 && !random;
            if (skipKMeans)
                name += "-single";

            // if not random, use the cache, if the cache lookup is true return the cache, if not run the whole process
            Colour4[] ret;
            if (!random && accentCache.TryGetValue(name, out ret))
                return ret;

            List<Rgba32> pixels = extractPixels(stream);
            // skip kmeans if the amount of clusters is 1, still doesnt match the first color when using 3 clusters
            if (skipKMeans)
                ret = [ Rgba32ToColour4(avgCluster(pixels)) ];
            else
            {
                // Not necessary to run Take since it already returns K colors (amount)
                ret = kMeans(pixels, amount, random: random).Select(Rgba32ToColour4).ToArray();
            }

            // uhhh yeahh
            accentCache.AddOrUpdate(name, ret, (_, _) => ret);

            return ret;
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

                    return avgCluster(cluster);
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
                // Add the average color as the first centroid
                centroids.Add(avgCluster(pixels));

                List<Rgba32> sortedPixels = pixels.OrderBy(pixel => pixel.R + pixel.G + pixel.B + pixel.A).ToList();

                int step = sortedPixels.Count / k;
                for (int i = 1; i < k; i++)
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

        private Rgba32 avgCluster(List<Rgba32> cluster) => new(
            (byte)cluster.Average(p => p.R * (p.A / 255f)),
            (byte)cluster.Average(p => p.G * (p.A / 255f)),
            (byte)cluster.Average(p => p.B * (p.A / 255f)),
            255 // fully opaque
        );

        public Colour4 Rgba32ToColour4(Rgba32 pixel) => new(pixel.R, pixel.G, pixel.B, pixel.A);
    }
}
