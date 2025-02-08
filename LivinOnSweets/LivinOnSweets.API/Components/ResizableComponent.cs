using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace LivinOnSweets.API.Components
{
    // Not finished really, but for now its a quick way to resize I guess
    public partial class ResizableComponent : Component, IDraggableComponent
    {
        protected ComponentContainer ComponentCntr;

        private Drawable content => ComponentCntr.Children.ElementAtOrDefault(1) ?? throw new ArgumentNullException();

        private DraggableComponent dragComponent;

        private Vector2 resizeStartSize;
        private Vector2 resizeStartMousePosition;
        private ResizeHandle resizeHandle;

        protected virtual bool IsResizing { get; private set; }

        public void BindTo(ComponentContainer target)
        {
            ComponentCntr = target;
            target.Add(new Container<ResizeHandle>()
            {
                Depth = -1,
                RelativeSizeAxes = Axes.Both,
                Child = resizeHandle = new ResizeHandle(Anchor.BottomRight, this)
            });
            dragComponent = target.GetComponent<DraggableComponent>();
        }

        // Events are fired through ResizeHandles, not through ComponentContainer
        public bool DragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (resizeHandle.IsHovered)
            {
                IsResizing = true;
                resizeStartSize = content.Size;
                resizeStartMousePosition = ComponentCntr.ToLocalSpace(e.ScreenSpaceMousePosition);
                return true;
            }

            return false;
        }

        public void Dragging(DragEvent e)
        {
            if (!IsResizing)
                return;

            Vector2 delta = ComponentCntr.ToLocalSpace(e.ScreenSpaceMousePosition) - resizeStartMousePosition;
            Vector2 newSize = resizeStartSize + delta;
            newSize = new Vector2(Math.Max(resizeHandle.Width, newSize.X), Math.Max(resizeHandle.Height, newSize.Y));

            // TODO
            if (content is Container container)
            {
                switch (container.AutoSizeAxes)
                {
                    case Axes.X:
                        float prevWidth = container.DrawWidth;
                        container.AutoSizeAxes &= ~Axes.X;
                        container.Width = prevWidth;
                        break;

                    case Axes.Y:
                        float prevHeight = container.DrawHeight;
                        container.AutoSizeAxes &= ~Axes.Y;
                        container.Height = prevHeight;
                        break;

                    case Axes.Both:
                        Vector2 prevSize = container.DrawSize;
                        container.AutoSizeAxes = Axes.None;
                        container.Size = prevSize;
                        break;
                }

            }

            content.ResizeTo(newSize, 200D, Easing.OutQuint);
        }

        public void DragEnd(DragEndEvent e) => IsResizing = false;

        // I added the interface because sometimes it seems to fail to fire the event, its probably
        // due to the drag blocks click flag or something, I still dont get it
        private partial class ResizeHandle : Box, IRequireHighFrequencyMousePosition
        {
            private readonly ResizableComponent controller;
            // Check if theres any Drag Component inside the CompCont, if there is, fire the events manually here
            // if not fire the events in the ComponentContainer
            private bool shouldFire => controller.dragComponent != null;

            public ResizeHandle(Anchor anchor, ResizableComponent component)
            {
                controller = component;

                Size = new Vector2(15);
                Colour = Colour4.Red;
                Alpha = 0.85f;

                Anchor = anchor;
                Origin = anchor;
            }

            protected override bool OnHover(HoverEvent e)
            {
                return true;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                if (shouldFire)
                    controller.DragStart(e);

                return shouldFire;
            }

            protected override void OnDrag(DragEvent e)
            {
                if (shouldFire)
                    controller.Dragging(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                if (shouldFire)
                    controller.DragEnd(e);
            }
        }
    }
}
