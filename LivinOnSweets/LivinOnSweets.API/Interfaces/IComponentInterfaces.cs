using LivinOnSweets.API.Containers;
using osu.Framework.Input.Events;

namespace LivinOnSweets.API.Interfaces
{
    // I should definitely think of better names n shi, also move these to anoter namespace or something but uhh
    // they gonna stay here because they are related to the components

    // Used to get plugged inside ComponentContainer
    // I'm not convinced with the name but uhh okay
    public interface IBindableComponent
    {
        public void BindTo(ComponentContainer target);
    }

    public interface IDraggableComponent : IBindableComponent
    {
        public bool DragStart(DragStartEvent e);

        public void Dragging(DragEvent e);

        public void DragEnd(DragEndEvent e);
    }
}
