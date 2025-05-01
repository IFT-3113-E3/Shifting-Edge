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
        public float knockbackForce;
        public Vector3 knockbackDirection;

        public DamageRequest(float damage, GameObject source, Vector3 hitPoint, float knockbackForce = 0f, Vector3 knockbackDirection = default)
        {
            this.knockbackDirection = knockbackDirection;
            this.knockbackForce = knockbackForce;
            this.damage = damage;
            this.source = source;
            this.hitPoint = hitPoint;
        }
    }

    public class EntityStatus : MonoBehaviour
    {
        [SerializeField] // Modifié pour être accessible dans l'inspecteur et depuis d'autres scripts
        public float maxHealth = 100f;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<DamageRequest> OnDamageTaken;
        public event Action<DamageRequest> OnDeath;
        public event Action<float> OnHealthChanged;

        private readonly List<IStatusEffect> _activeEffects = new();

        public DeathSequence deathSequence;

        
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
        
        public void SetMaxHealth(float value)
        {
            maxHealth = value;
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }
        
        public void SetCurrentHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
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
            OnHealthChanged?.Invoke(CurrentHealth);

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
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private void Die(DamageRequest damageRequest)
        {
            if (IsDead) return;
            IsDead = true;
            OnDeath?.Invoke(damageRequest);
            // gameObject.SetActive(false);
            deathSequence.TriggerDeath();
        }

        public void Revive()
        {
            IsDead = false;
            CurrentHealth = maxHealth; // Utilisera la valeur actuelle de maxHealth
        }
    }
}
