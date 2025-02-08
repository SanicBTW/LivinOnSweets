using LivinOnSweets.API.Interfaces;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;

namespace LivinOnSweets.API.Containers
{
    public partial class ComponentContainer : Container
    {
        protected Container<Component> Components;

        public ComponentContainer()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(Components = new Container<Component>()
            {
                Depth = 99
            });
        }

        public void AddComponent(Component component)
        {
            if (component is not IBindableComponent iComponent)
                throw new ArgumentException();

            iComponent.BindTo(this);
            Components.Add(component);
        }

        public bool HasComponent<T>(out T[] components)
        {
            IEnumerable<T> lookup = Components.ChildrenOfType<T>();
            components = lookup as T[] ?? lookup.ToArray();
            return components.Length > 0;
        }

        public T GetComponent<T>()
        {
            if (!HasComponent(out T[] components))
                return default;

            return components.First();
        }

        internal bool ComponentDragBlocksClick;
        public override bool DragBlocksClick => ComponentDragBlocksClick;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!HasComponent(out IDraggableComponent[] draggables))
                return false;

            // TODO! Fix this behaviour, Drag is prioritized Resize, and Resizable gets its own events fired on another place
            bool handled = false;
            foreach (IDraggableComponent draggable in draggables)
            {
                if (draggable.DragStart(e))
                {
                    handled = true;
                    break;
                }
            }

            return handled;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (!HasComponent(out IDraggableComponent[] draggables))
                return;

            foreach (IDraggableComponent draggable in draggables)
                draggable.Dragging(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (!HasComponent(out IDraggableComponent[] draggables))
                return;

            foreach (IDraggableComponent draggable in draggables)
                draggable.DragEnd(e);
        }
    }
}
