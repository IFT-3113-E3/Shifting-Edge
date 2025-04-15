namespace Status
{
    public interface IStatusEffect
    {
        string Id { get; }
        bool IsFinished { get; }
        void OnApply(StatusEffectManager manager);
        void Tick(float deltaTime);
        void OnRemove(StatusEffectManager manager);
    }
}