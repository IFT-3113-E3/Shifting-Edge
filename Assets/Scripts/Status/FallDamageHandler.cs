using UnityEngine;
using Status;

[RequireComponent(typeof(EntityStatus))]
public class FallDamageHandler : MonoBehaviour
{
    [Header("Fall Damage Settings")]
    public float minFallDistance = 3f;
    public float damageMultiplier = 10f;

    private EntityStatus entityStatus;
    private CharacterController characterController;
    private bool isFalling = false;
    private float startFallHeight;
    private float lastYPosition;

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (!grounded && !isFalling)
        {
            isFalling = true;
            startFallHeight = transform.position.y;
        }
        else if (grounded && isFalling)
        {
            isFalling = false;
            float fallDistance = startFallHeight - transform.position.y;

            if (fallDistance > minFallDistance)
            {
                float damage = (fallDistance - minFallDistance) * damageMultiplier;
                ApplyFallDamage(damage);
            }
        }
    }

    private void ApplyFallDamage(float damage)
    {
        DamageRequest fallDamage = new DamageRequest(
            damage,
            null,
            transform.position
        );

        entityStatus.ApplyDamage(fallDamage);
    }

    private bool IsGrounded()
    {
        if (characterController != null)
        {
            return characterController.isGrounded;
        }
        else
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
            return Physics.Raycast(ray, 1.0f);
        }
    }
}
