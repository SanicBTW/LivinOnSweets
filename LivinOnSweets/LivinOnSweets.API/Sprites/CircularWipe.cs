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
    public partial class CircularWipe : Sprite
    {
        protected override DrawNode CreateDrawNode() => new CircularWipeNode(this);

        private float progress;

        // The progress of the wipe
        public float Progress
        {
            get => progress;
            set
            {
                progress = Math.Clamp(value, 0, 1);
                Invalidate(Invalidation.DrawNode);
            }
        }

        // If the content behind should be shown while animating the wipe
        public bool Reveal;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            Texture = renderer.WhitePixel;
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"CircularWipe");
        }

        private partial class CircularWipeNode : SpriteDrawNode
        {
            public new CircularWipe Source => (CircularWipe)base.Source;

            [CanBeNull] private IUniformBuffer<CircularWipeParameters> parametersBuffer;
            private float progress;
            private bool reveal;

            public CircularWipeNode(CircularWipe source) : base(source) { }

            public override void ApplyState()
            {
                base.ApplyState();

                progress = Source.Progress;
                reveal = Source.Reveal;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<CircularWipeParameters>();
                parametersBuffer.Data = new CircularWipeParameters()
                {
                    Progress = progress,
                    Reveal = reveal
                };

                shader.BindUniformBlock("m_WipeParameters", parametersBuffer);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct CircularWipeParameters
        {
            public UniformFloat Progress;
            public UniformBool Reveal;
            private readonly UniformPadding8 pad1;
        }
    }
}
