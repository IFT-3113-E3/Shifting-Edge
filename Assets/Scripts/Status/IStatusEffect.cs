namespace Status
{
    public interface IStatusEffect
    {
        string Id { get; }
        bool IsFinished { get; }
        void OnApply(EntityStatus manager);
        void Tick(float deltaTime);
        void OnRemove(EntityStatus manager);
    }
}