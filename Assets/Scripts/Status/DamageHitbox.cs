using UnityEngine;

namespace Status
{
    public class DamageHitbox : MonoBehaviour
    {
        public float damage = 25f;
        public GameObject owner;

        public string[] damageTags = { "Enemy", "Player" };

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;

            if (damageTags.Length > 0 && !IsTagAllowed(other.tag)) return;

            var status = other.GetComponent<EntityStatus>();
            if (status != null)
            {
                status.ApplyDamage(new DamageRequest
                {
                    damage = damage,
                    hitPoint = other.transform.position,
                    source = owner
                });
                Debug.Log($"{owner?.name ?? "Something"} dealt {damage} to {other.name}");
            }
        }

        private bool IsTagAllowed(string tag)
        {
            foreach (var allowedTag in damageTags)
            {
                if (tag == allowedTag) return true;
            }
            return false;
        }
    }
}