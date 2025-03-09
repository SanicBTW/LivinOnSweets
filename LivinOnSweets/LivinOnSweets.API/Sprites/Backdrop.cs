using System.Runtime.InteropServices;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace LivinOnSweets.API.Sprites
{
    public partial class Backdrop : Sprite
    {
        protected override DrawNode CreateDrawNode() => new BackdropDrawNode(this);

        public bool Running { get; private set; }

        // A bindable to be able to transform it
        // A negative speed will go the opposite direction
        // +x => left to right / -x => right to left | +y => up to down / -y => down to up
        public Bindable<Vector2> ScrollSpeed = new();

        private bool startImmediately;

        public Backdrop(float speed = 1F, bool startOnLoad = false)
        {
            ScrollSpeed.Value = new Vector2(speed);

            RelativeSizeAxes = Axes.Both;
            startImmediately = startOnLoad;
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Invalidate every time the scroll speed changes
            ScrollSpeed.BindValueChanged(_ => Invalidate(Invalidation.DrawNode));

            if (startImmediately)
                Start();
        }


        private partial class BackdropDrawNode : SpriteDrawNode
        {
            public new Backdrop Source => (Backdrop)base.Source;

            [CanBeNull] private IUniformBuffer<BackdropTileParameters> parametersBuffer;
            private Vector2 tileScale;
            private Vector2 scrollSpeed;

            public BackdropDrawNode(Backdrop source) : base(source) { }

            public override void ApplyState()
            {
                base.ApplyState();

                tileScale = new Vector2(
                    Source.DrawWidth / Texture.DisplayWidth,
                    Source.DrawHeight / Texture.DisplayHeight
                );

                if (!Source.Running)
                    scrollSpeed = Vector2.Zero;
                else
                    scrollSpeed = Source.ScrollSpeed.Value * 0.001F;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<BackdropTileParameters>();
                parametersBuffer.Data = new BackdropTileParameters()
                {
                    TileScale = tileScale,
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
                public UniformVector2 TileScale; // 8
                public UniformVector2 ScrollSpeed; // 16
                public UniformFloat Time; // 20
                private readonly UniformPadding12 pad1; // 32
            }
        }
    }
}
