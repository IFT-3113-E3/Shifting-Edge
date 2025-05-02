using System;
using UnityEngine;

namespace Enemy.IceBoss
{
    public class BossRoomTrigger : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private AudioSource audiosource;

        private bool _shouldSpawn = true;

        private void Start()
        {
            if (GameManager.Instance.GameSession.GameProgression.HasDefeated("IceBoss"))
            {
                _shouldSpawn = false;
                return;
            }
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
            if (!_shouldSpawn)
                return;
            if (boss.Context.shouldActivate)
                return;
            if (!other.CompareTag("Player")) return;
            boss.Context.shouldActivate = true;
            Debug.Log("[Boss Trigger] Activated boss via trigger!");

            audiosource.Play();
        }
    }

}