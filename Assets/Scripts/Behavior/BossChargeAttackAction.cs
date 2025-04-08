using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BossChargeAttackAction", story: "[Boss] charges attack on [Player]", category: "Action", id: "a7e9b6d63152a07884d06b792bf3a87d")]
public partial class BossChargeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Boss;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    private Vector3 currentAimDir;
    private Vector3 targetPosition;
    private float aimSpeed = 5f; // Adjust speed as needed
    
    private float aimTime = 5f; // Time to aim at the player
    private float aimStartTime;
    
    protected override Status OnStart()
    {
        Transform bossTransform = Boss.Value.transform;

        currentAimDir = bossTransform.forward;
        aimStartTime = Time.time;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Transform bossTransform = Boss.Value.transform;
        Transform playerTransform = Player.Value.transform;
        
        // slowly aim at the player
        targetPosition = playerTransform.position;
        Vector3 targetDir = (targetPosition - bossTransform.position).normalized;
        currentAimDir = Vector3.RotateTowards(currentAimDir, targetDir, aimSpeed * Time.deltaTime, 0.0f);
        currentAimDir.y = 0;

        // Rotate the boss towards the target direction
        bossTransform.rotation = Quaternion.LookRotation(currentAimDir);
        
        // Check if the boss has aimed for long enough
        if (Time.time - aimStartTime >= aimTime)
        {
            // Perform the charge attack
            // Here you can add the logic for the charge attack, e.g., moving towards the player
            // For now, we just return success
            return Status.Success;
        }
        
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

