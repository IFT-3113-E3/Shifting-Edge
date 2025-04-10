using System;
using System.Collections.Generic;
using UnityEngine;

namespace Status
{
    
    public struct DamageRequest
    {
        public float damage;
        public GameObject source;
        public Vector3 hitPoint;

        public DamageRequest(float damage, GameObject source, Vector3 hitPoint)
        {
            this.damage = damage;
            this.source = source;
            this.hitPoint = hitPoint;
        }
    }
    
    public class EntityStatus : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<DamageRequest> OnDamageTaken;
        public event Action<DamageRequest> OnDeath;
        
        private readonly List<IStatusEffect> _activeEffects = new();


        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Tick(Time.deltaTime);
                if (effect.IsFinished)
                {
                    effect.OnRemove(this);
                    _activeEffects.RemoveAt(i);
                }
            }
        }
        
        public void AddEffect(IStatusEffect effect)
        {
            if (HasEffect(effect.Id)) return;

            _activeEffects.Add(effect);
            effect.OnApply(this);
        }

        public bool HasEffect(string id)
        {
            return _activeEffects.Exists(e => e.Id == id);
        }

        public void RemoveEffect(string id)
        {
            int index = _activeEffects.FindIndex(e => e.Id == id);
            if (index >= 0)
            {
                _activeEffects[index].OnRemove(this);
                _activeEffects.RemoveAt(index);
            }
        }

        public void ApplyDamage(DamageRequest damageRequest)
        {
            if (IsDead) return;

            var amount = damageRequest.damage;
            
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            OnDamageTaken?.Invoke(damageRequest);

            foreach (var reaction in GetComponents<IHitReaction>())
                reaction.ReactToHit(damageRequest);

            if (CurrentHealth <= 0)
            {
                Die(damageRequest);
            }
        }


        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }

        private void Die(DamageRequest damageRequest)
        {
            if (IsDead) return;
            IsDead = true;
            OnDeath?.Invoke(damageRequest);
            // gameObject.SetActive(false);
        }

        public void Revive()
        {
            IsDead = false;
            CurrentHealth = maxHealth;
        }
    }
}