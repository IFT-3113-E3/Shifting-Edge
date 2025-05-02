using UnityEngine;

namespace Status
{
    public class PassiveRegenerationApplier : MonoBehaviour
    {
        public float healthPerSecond = 5f;
        public float waitTime = 5f;
        
        private EntityStatus _entityStatus;
        private float _timeSinceLastHit = 0f;
        private bool _isRegenerating = false;

        private void Awake()
        {
            _entityStatus = GetComponent<EntityStatus>();
        }
        
        private void OnEnable()
        {
            _entityStatus.OnDamageTaken += OnHit;
        }
        
        private void OnDisable()
        {
            _entityStatus.OnDamageTaken -= OnHit;
        }
        
        private void Update()
        {
            if (_entityStatus.IsDead) return;
            if (_entityStatus.CurrentHealth >= _entityStatus.maxHealth)
            {
                _timeSinceLastHit = 0f;
                
                ClearEffect();
            }

            if (_isRegenerating) return;
            _timeSinceLastHit += Time.deltaTime;

            if (_timeSinceLastHit >= waitTime)
            {
                Debug.Log("Regenerating health");
                _timeSinceLastHit = 0f;
                _isRegenerating = true;
                ApplyPassiveRegeneration();
            }
        }
        
        private void ApplyPassiveRegeneration()
        {
            if (_entityStatus.CurrentHealth < _entityStatus.maxHealth)
            {
                _entityStatus.AddEffect(new RegenerationStatusEffect(healthPerSecond, float.MaxValue, true));
            }
        }
        
        private void ClearEffect()
        {
            _isRegenerating = false;
            _entityStatus.RemoveEffect("Regeneration");
        }
        
        private void OnHit(DamageRequest damageRequest)
        {
            _timeSinceLastHit = 0f;
            if (_isRegenerating)
            {
                ClearEffect();
            }
        }
    }
}