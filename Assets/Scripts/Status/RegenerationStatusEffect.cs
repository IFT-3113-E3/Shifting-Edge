namespace Status
{
    public class RegenerationStatusEffect : IStatusEffect
    {
        private EntityStatus _entityStatus;
        private float _duration;
        private float _healthPerSecond;
        private float _elapsedTime;
        private bool _isContinuous;
        
        public RegenerationStatusEffect(float healthPerSecond, float duration, bool continuous = false)
        {
            _duration = duration;
            _healthPerSecond = healthPerSecond;
            _isContinuous = continuous;
        }
        
        public string Id => "Regeneration";
        public bool IsFinished => _duration <= 0;

        public void OnApply(EntityStatus manager)
        {
            _entityStatus = manager;
        }

        public void Tick(float deltaTime)
        {
            if (_entityStatus == null) return;

            _duration -= deltaTime;

            if (!_isContinuous)
            {
                _elapsedTime += deltaTime;
            
                if (_elapsedTime > 1f)
                {
                    _elapsedTime = 0;
                    _entityStatus.Heal(_healthPerSecond);
                }                
            }
            else
            {
                _entityStatus.Heal(_healthPerSecond * deltaTime);
            }
        }

        public void OnRemove(EntityStatus manager)
        {
            _entityStatus = null;
        }
    }
}