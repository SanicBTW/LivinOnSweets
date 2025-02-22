using System.Runtime.InteropServices;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;

namespace LivinOnSweets.API.Sprites
{
    public partial class Backdrop : Sprite
    {
        protected override DrawNode CreateDrawNode() => new BackdropDrawNode(this);

        public readonly double Duration;

        public bool Running { get; private set; }

        public Backdrop(double duration = 2000D)
        {
            RelativeSizeAxes = Axes.Both;
            Duration = duration;
        }

        public void Start()
        {
            if (Running)
                return;

            Running = true;
            Invalidate(Invalidation.DrawNode);
        }

        public void Freeze()
        {
            if (!Running)
                return;

            Running = false;
            Invalidate(Invalidation.DrawNode);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(@"Backdrop", @"Backdrop");
        }

        private partial class BackdropDrawNode : SpriteDrawNode
        {
            public new Backdrop Source => (Backdrop)base.Source;

            [CanBeNull] private IUniformBuffer<BackdropTileParameters> parametersBuffer;
            private float tileScaleX = 1;
            private float tileScaleY = 1;
            private float scrollSpeed;

            public BackdropDrawNode(Backdrop source) : base(source) { }

            public override void ApplyState()
            {
                base.ApplyState();

                tileScaleX = Source.DrawWidth / Texture.DisplayWidth;
                tileScaleY = Source.DrawHeight / Texture.DisplayHeight;

                if (!Source.Running || Source.Duration <= 0)
                    scrollSpeed = 0;
                else
                    scrollSpeed = (0.001f / (float)(Source.Duration / 1000.0));
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<BackdropTileParameters>();
                parametersBuffer.Data = new BackdropTileParameters()
                {
                    TileScaleX = tileScaleX,
                    TileScaleY = tileScaleY,
                    ScrollSpeed = scrollSpeed,
                    Time = (float)Source.Time.Current
                };

                shader.BindUniformBlock("m_GridUniforms", parametersBuffer);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct BackdropTileParameters
            {
                public UniformFloat TileScaleX; // 4
                public UniformFloat TileScaleY; // 8
                public UniformFloat ScrollSpeed; // 12
                public UniformFloat Time; // 16
                private readonly UniformPadding12 pad1; // 28
                private readonly UniformPadding4 pad2; // 32
            }
        }
    }
}
