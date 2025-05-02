using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Status;
using Random = UnityEngine.Random;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    ReturnToArea
}

public class EnemyAI : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isRunning");
    private static readonly int Attack1 = Animator.StringToHash("lightAttack");
    private static readonly int BreakAttack = Animator.StringToHash("breakAttack");

    [Header("AI Settings")]
    public EnemyState currentState = EnemyState.Idle;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float patrolRadius = 10f;
    public float idleTime = 2f;
    public float maxDistanceFromSpawnArea = 30f; // How far the enemy can wander from its spawn area

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int damage = 10;
    public float attackSpeed = 2f;

    [Header("Visual Feedback")]
    public GameObject attackVFX;

    // Private references
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Vector3 spawnAreaCenter; // Center of the spawn area
    private float spawnAreaRadius;   // Radius of the spawn area
    private Vector3 currentPatrolTarget;
    private bool isInitialized = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void Initialize(Transform playerTransform, Vector3 areaCenter, float areaRadius)
    {
        player = playerTransform;
        spawnAreaCenter = areaCenter;
        spawnAreaRadius = areaRadius;
        patrolRadius = Mathf.Min(patrolRadius, areaRadius * 0.8f); // Make sure patrol radius isn't larger than spawn area

        // Configure NavMesh agent
        if (agent)
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }

        Debug.Log($"Enemy AI initialized with player: {player.name}, spawn area center: {spawnAreaCenter}, radius: {spawnAreaRadius}");

        // Start AI behavior
        isInitialized = true;
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (enabled && isInitialized)
        {
            yield return StartCoroutine(currentState.ToString());
        }
    }

    IEnumerator Idle()
    {
        // Stop and play idle animation
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.ResetTrigger(Attack1);
            animator.SetTrigger(BreakAttack);
            animator.SetBool(IsMoving, false);
        }

        // Wait for idle time
        float timer = 0;
        while (timer < idleTime)
        {
            timer += Time.deltaTime;

            // Check if player is in range
            if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
            {
                currentState = EnemyState.Chase;
                yield break;
            }

            // Check if enemy has wandered too far from spawn area
            if (Vector3.Distance(transform.position, spawnAreaCenter) > maxDistanceFromSpawnArea)
            {
                currentState = EnemyState.ReturnToArea;
                yield break;
            }

            yield return null;
        }

        // Switch to patrol state
        currentState = EnemyState.Patrol;
    }

    IEnumerator Patrol()
    {
        // Find a random patrol point within the spawn area
        bool foundPosition = false;
        int attempts = 0;

        while (!foundPosition && attempts < 5)
        {
            // Random direction within patrol radius
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0;
            Vector3 targetPosition = transform.position + randomDirection;

            // Make sure it's within the spawn area
            if (Vector3.Distance(targetPosition, spawnAreaCenter) > spawnAreaRadius)
            {
                // Point outside spawn area, adjust it
                Vector3 directionToTarget = (targetPosition - spawnAreaCenter).normalized;
                targetPosition = spawnAreaCenter + directionToTarget * (spawnAreaRadius * 0.8f);
            }

            // Check if point is on NavMesh
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                currentPatrolTarget = hit.position;
                foundPosition = true;

                // Move to point
                if (agent != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(currentPatrolTarget);

                    if (animator != null)
                    {
                        animator.SetBool(IsMoving, true);
                    }
                }
            }

            attempts++;
        }

        // If no valid position found, return to idle
        if (!foundPosition)
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        // Wait until destination reached or player detected
        while (agent != null && Vector3.Distance(transform.position, currentPatrolTarget) > agent.stoppingDistance + 0.5f)
        {
            // Check if player is in range
            if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
            {
                currentState = EnemyState.Chase;
                yield break;
            }

            // Check if enemy has wandered too far from spawn area
            if (Vector3.Distance(transform.position, spawnAreaCenter) > maxDistanceFromSpawnArea)
            {
                currentState = EnemyState.ReturnToArea;
                yield break;
            }

            yield return null;
        }

        // Destination reached, return to idle
        currentState = EnemyState.Idle;
    }

    IEnumerator Chase()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        // Chase player
        if (agent != null)
        {
            agent.isStopped = false;

            if (animator != null)
            {
                animator.ResetTrigger(Attack1);
                animator.SetTrigger(BreakAttack);
                animator.SetBool(IsMoving, true);
            }
        }

        while (true)
        {
            // Update destination to player position
            if (agent != null && player != null)
            {
                agent.SetDestination(player.position);
            }

            // Check if player is in attack range
            float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

            if (distanceToPlayer < attackRange)
            {
                currentState = EnemyState.Attack;
                yield break;
            }

            // Check if player is out of detection range
            if (distanceToPlayer > detectionRange * 1.5f)
            {
                currentState = EnemyState.Patrol;
                yield break;
            }

            // Check if enemy has wandered too far from spawn area
            if (Vector3.Distance(transform.position, spawnAreaCenter) > maxDistanceFromSpawnArea)
            {
                currentState = EnemyState.ReturnToArea;
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator Attack()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        // Stop to attack
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Face player
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
            animator.ResetTrigger(BreakAttack);
            animator.SetTrigger(Attack1);
        }

        // Wait for animation to start
        yield return new WaitForSeconds(0.5f);

        // Apply damage if player is still in range
        if (player != null && Vector3.Distance(transform.position, player.position) < attackRange * 1.2f)
        {
            // Show attack VFX
            if (attackVFX != null)
            {
                GameObject vfx = Instantiate(attackVFX, player.position + Vector3.up, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            // Apply damage to player
            if (player.TryGetComponent<EntityStatus>(out var playerHealth))
            {
                DamageRequest damageRequest = new(damage, gameObject, player.position);
                playerHealth.ApplyDamage(damageRequest);
            }
        }

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);

        // Determine next state
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < attackRange)
            {
                // Continue attacking
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer < detectionRange)
            {
                // Chase player
                currentState = EnemyState.Chase;
            }
            else
            {
                // Return to patrol
                currentState = EnemyState.Patrol;
            }

            // Check if enemy has wandered too far from spawn area
            if (Vector3.Distance(transform.position, spawnAreaCenter) > maxDistanceFromSpawnArea)
            {
                currentState = EnemyState.ReturnToArea;
            }
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    IEnumerator ReturnToArea()
    {
        // Find a point within the spawn area
        Vector3 returnPoint = spawnAreaCenter;
        
        // Try to find a valid NavMesh position near the center
        if (NavMesh.SamplePosition(spawnAreaCenter, out NavMeshHit hit, spawnAreaRadius * 0.5f, NavMesh.AllAreas))
        {
            returnPoint = hit.position;
        }

        // Move back to spawn area
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(returnPoint);
            
            if (animator != null)
            {
                animator.SetBool(IsMoving, true);
            }
        }

        // Wait until we're back in the area or player is detected
        while (Vector3.Distance(transform.position, spawnAreaCenter) > spawnAreaRadius * 0.7f)
        {
            // Check if player is in range
            if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
            {
                currentState = EnemyState.Chase;
                yield break;
            }
            
            yield return null;
        }

        // Back in spawn area, return to patrol
        currentState = EnemyState.Patrol;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        
        // Draw spawn area if initialized
        if (isInitialized)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnAreaCenter, spawnAreaRadius);
            
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
            Gizmos.DrawWireSphere(spawnAreaCenter, maxDistanceFromSpawnArea);
        }
        
        // Draw current patrol target if in patrol state
        if (currentState == EnemyState.Patrol)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentPatrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
        }
    }
}