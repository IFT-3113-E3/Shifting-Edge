using System;
using UnityEngine;

namespace Enemy.IceBoss
{
    [RequireComponent(typeof(Rigidbody))]
    public class BossMovementController : MonoBehaviour
    {
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("[BossMovementController] Rigidbody not found!");
                return;
            }
        }

        private void MoveTowardsTarget(Transform target, float speed, float rotationSpeed)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            transform.position += direction * speed * Time.deltaTime;
        }

        private void LateUpdate()
        {
            // snap the rotation to 8 directions
            Vector3 forward = transform.forward;
            float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45) * 45;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}