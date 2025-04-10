using System.Collections.Generic;
using UnityEngine;

namespace Status
{
    [RequireComponent(typeof(EntityStatus))]
    public class StatusEffectManager : MonoBehaviour
    {
        private readonly List<IStatusEffect> activeEffects = new();
        public EntityStatus Entity { get; private set; }

        private void Awake()
        {
            Entity = GetComponent<EntityStatus>();
        }

        // private void Update()
        // {
        //     for (int i = activeEffects.Count - 1; i >= 0; i--)
        //     {
        //         var effect = activeEffects[i];
        //         effect.Tick(Time.deltaTime);
        //         if (effect.IsFinished)
        //         {
        //             effect.OnRemove(this);
        //             activeEffects.RemoveAt(i);
        //         }
        //     }
        // }
        //
        // public void AddEffect(IStatusEffect effect)
        // {
        //     if (HasEffect(effect.Id)) return;
        //
        //     activeEffects.Add(effect);
        //     effect.OnApply(this);
        // }
        //
        // public bool HasEffect(string id)
        // {
        //     return activeEffects.Exists(e => e.Id == id);
        // }
        //
        // public void RemoveEffect(string id)
        // {
        //     int index = activeEffects.FindIndex(e => e.Id == id);
        //     if (index >= 0)
        //     {
        //         activeEffects[index].OnRemove(this);
        //         activeEffects.RemoveAt(index);
        //     }
        // }
    }
}