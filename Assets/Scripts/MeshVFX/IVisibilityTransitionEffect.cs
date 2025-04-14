namespace MeshVFX
{
    public interface IVisibilityTransitionEffect
    {
        bool IsTransitioning { get; }
        bool IsVisible { get; }

        void SetSpeedFactor(float speedFactor);
        
        void Show();
        void Hide();
        void Cancel();
    }
}