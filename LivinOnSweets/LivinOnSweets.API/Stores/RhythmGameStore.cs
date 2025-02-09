using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace LivinOnSweets.API.Stores
{
    public class RhythmGameStore : AnimatedPixelArtTextureStore
    {
        public readonly ITrackStore TrackStore;
        public readonly ISampleStore SampleStore;

        public RhythmGameStore(IRenderer renderer, IResourceStore<byte[]> resources, AudioManager audio)
            : base(renderer, new TextureLoaderStore(new RhythmGameNamespace(resources)), false,
                true, 1)
        {
            // Kind of lame to do another instace but uh yea
            RhythmGameNamespace gameNamespace = new RhythmGameNamespace(resources);

            // This is to populate the namespace resources into the audio manager, so we have to ways to gather a sound
            // 1. through this store, by resolved attr or argument on load bdl
            // 2. through the specified stores (track/sample) resolved or load bdl
            // 3. through audio managher, resolved or load bdl, accessing audio.(tracks/sample)
            TrackStore = audio.GetTrackStore(gameNamespace);
            SampleStore = audio.GetSampleStore(gameNamespace);
        }
    }

    public class RhythmGameNamespace(IResourceStore<byte[]> store)
        : PreservingNamespaceResourceStore<byte[]>(store, "RhythmGame");
}
