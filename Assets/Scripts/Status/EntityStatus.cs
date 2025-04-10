using System;
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
        [SerializeField] // Modifié pour être accessible dans l'inspecteur et depuis d'autres scripts
        public float maxHealth = 100f;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, GameObject> OnDamageTaken;
        public event Action<GameObject> OnDeath;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(DamageRequest damageRequest)
        {
            if (IsDead) return;

            var amount = damageRequest.damage;
            var source = damageRequest.source;
            
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            OnDamageTaken?.Invoke(amount, source);

            foreach (var reaction in GetComponents<IHitReaction>())
                reaction.ReactToHit(damageRequest);

            if (CurrentHealth <= 0)
            {
                Die(source);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }

        private void Die(GameObject source)
        {
            if (IsDead) return;
            IsDead = true;
            OnDeath?.Invoke(source);
            gameObject.SetActive(false);
        }

        public void Revive()
        {
            IsDead = false;
            CurrentHealth = maxHealth; // Utilisera la valeur actuelle de maxHealth
        }
    }
}