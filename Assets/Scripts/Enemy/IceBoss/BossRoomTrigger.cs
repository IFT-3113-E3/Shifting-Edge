using System;
using UnityEngine;

namespace Enemy.IceBoss
{
    public class BossRoomTrigger : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private AudioSource audiosource;

        private void Start()
        {
            if (boss == null)
            {
                boss = FindFirstObjectByType<BossController>();
                if (boss == null)
                {
                    Debug.LogError("[Boss Trigger] BossController not found in scene!");
                    return;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (boss.Context.shouldActivate)
                return;
            if (other.CompareTag("Player"))
            {
                boss.Context.shouldActivate = true;
                Debug.Log("[Boss Trigger] Activated boss via trigger!");

                audiosource.Play();
            }
        }
    }

}