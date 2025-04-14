using UnityEngine;

namespace Status
{
    public interface IHitReaction
    {
        void ReactToHit(DamageRequest damageRequest);
    }
}