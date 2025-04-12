namespace MeshVFX
{
    public interface IVisibilityTransitionEffect
    {
        bool IsTransitioning { get; }
        bool IsVisible { get; }

        void Show();
        void Hide();
        void Cancel();
    }
}