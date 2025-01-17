namespace LivinOnSweets.API.Interfaces
{
    // This container will prevent the editor container from closing it and its children or parents
    // depending on the place where it was placed, BUT it will always prevent the container from being closed
    public interface ISlideContainerCloseBlock
    {
        // dumb ahh variable naming, basically quick flag to let the editor
        // container know which sliders can it close when clicked outside of it
        public bool ClickOutClosesContainer { get; set; }
    }
}
