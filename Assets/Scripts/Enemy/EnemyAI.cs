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
    Attack
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

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int damage = 10;
    public float attackSpeed = 2f;

    [Header("Visual Feedback")]
    public GameObject attackVFX;

    // Références privées
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Vector3 startPosition;
    // private bool canAttack = true;
    private Vector3 currentPatrolTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
    }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;

        // Configurer l'agent NavMesh
        if (agent)
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }

        // Démarrer le comportement
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (enabled)
        {
            yield return StartCoroutine(currentState.ToString());
        }
    }

    IEnumerator Idle()
    {
        // S'arrêter et jouer l'animation d'inactivité
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

        // Attendre un certain temps
        float timer = 0;
        while (timer < idleTime)
        {
            timer += Time.deltaTime;

            // Vérifier si le joueur est à portée
            if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
            {
                currentState = EnemyState.Chase;
                yield break;
            }

            yield return null;
        }

        // Passer à l'état de patrouille
        currentState = EnemyState.Patrol;
    }

    IEnumerator Patrol()
    {
        // Trouver un point de patrouille aléatoire
        bool foundPosition = false;
        int attempts = 0;

        while (!foundPosition && attempts < 5)
        {
            // Position aléatoire autour du point de départ
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0;
            Vector3 targetPosition = startPosition + randomDirection;

            // Vérifier si le point est sur la NavMesh
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                currentPatrolTarget = hit.position;
                foundPosition = true;

                // Déplacer vers le point
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

        // Si on n'a pas trouvé de position valide, rester inactif
        if (!foundPosition)
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        // Attendre d'atteindre la destination ou de détecter le joueur
        while (Vector3.Distance(transform.position, currentPatrolTarget) > agent.stoppingDistance + 0.5f)
        {
            // Vérifier si le joueur est à portée
            if (player != null && Vector3.Distance(transform.position, player.position) < detectionRange)
            {
                currentState = EnemyState.Chase;
                yield break;
            }

            yield return null;
        }

        // Destination atteinte, retour à l'état inactif
        currentState = EnemyState.Idle;
    }

    IEnumerator Chase()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        // Poursuivre le joueur
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
            // Mettre à jour la destination vers le joueur
            if (agent != null && player != null)
            {
                agent.SetDestination(player.position);
            }

            // Vérifier si on peut attaquer
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < attackRange)
            {
                currentState = EnemyState.Attack;
                yield break;
            }

            // Vérifier si le joueur est hors de portée de détection
            if (distanceToPlayer > detectionRange * 1.5f)
            {
                currentState = EnemyState.Patrol;
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

        // S'arrêter pour attaquer
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Se tourner vers le joueur
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        // Lancer l'animation d'attaque
        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
            animator.ResetTrigger(BreakAttack);
            animator.SetTrigger(Attack1);
        }

        // Effectuer des dégâts après un court délai
        yield return new WaitForSeconds(0.5f); // Délai pour que l'animation commence

        // Si le joueur est toujours à portée, infliger des dégâts
        if (player != null && Vector3.Distance(transform.position, player.position) < attackRange * 1.2f)
        {
            // Afficher l'effet visuel
            if (attackVFX != null)
            {
                GameObject vfx = Instantiate(attackVFX, player.position + Vector3.up, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            // Infliger des dégâts au joueur EntityStatus
            EntityStatus playerHealth = player.GetComponent<EntityStatus>();
            if (playerHealth != null)
            {
                DamageRequest damageRequest = new DamageRequest(damage, gameObject, player.position);
                playerHealth.ApplyDamage(damageRequest);
            }
        }

        // Attendre la fin de l'animation
        yield return new WaitForSeconds(attackCooldown);

        // Déterminer le prochain état
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < attackRange)
            {
                // Continuer d'attaquer
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer < detectionRange)
            {
                // Poursuivre le joueur
                currentState = EnemyState.Chase;
            }
            else
            {
                // Retourner à la patrouille
                currentState = EnemyState.Patrol;
            }
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    // Pour le débogage
    void OnDrawGizmosSelected()
    {
        // Dessiner la zone de détection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Dessiner la zone d'attaque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Dessiner la zone de patrouille
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
}
