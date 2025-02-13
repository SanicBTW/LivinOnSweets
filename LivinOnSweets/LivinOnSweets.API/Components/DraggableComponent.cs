using LivinOnSweets.API.Containers;
using LivinOnSweets.API.Interfaces;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace LivinOnSweets.API.Components
{
    // This was a container before, now a component for full flexibility :sunglasses:
    public partial class DraggableComponent : Component, IDraggableComponent
    {
        protected ComponentContainer ComponentCntr;

        protected new bool DragBlocksClick
        {
            get => ComponentCntr.ComponentDragBlocksClick;
            set => ComponentCntr.ComponentDragBlocksClick = value;
        }

        private MouseButtonEventManager dragButtonManager;

        public virtual bool IsDragging { get; private set; }

        public virtual void BindTo(ComponentContainer target) => ComponentCntr = target;

        // These functions are hookups to the parent container events, middlewares you could say
        public virtual bool DragStart(DragStartEvent e)
        {
            if (IsDragging || e.Button != MouseButton.Left)
                return false;

            IsDragging = true;
            dragButtonManager = GetContainingInputManager().AsNonNull().GetButtonEventManagerFor(e.Button);

            return true;
        }

        public virtual void Dragging(DragEvent e)
        {
            if (!IsDragging)
                return;

            ComponentCntr.Position += e.Delta;

            DragBlocksClick |= Math.Abs(e.MouseDownPosition.LengthFast - e.MousePosition.LengthFast) > dragButtonManager.ClickDragDistance;
        }

        public virtual void DragEnd(DragEndEvent e)
        {
            DragBlocksClick = false;
            dragButtonManager = null;
            IsDragging = false;
        }
    }
}
