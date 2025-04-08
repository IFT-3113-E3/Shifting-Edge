using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FlyToPlayer", story: "[Agent] flies to [player]", category: "Action", id: "66175111188a4113a27fa5c8f9151320")]
public class FlyToPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Player;
    protected override Status OnStart()
    {
        if (Agent.Value == null || Player.Value == null)
        {
            Debug.LogError("Agent or Player is not set.");
            return Status.Failure;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Player.Value == null)
        {
            Debug.LogError("Agent or Player is not set.");
            return Status.Failure;
        }
        var agentTransform = Agent.Value.transform;
        var playerTransform = Player.Value;
        var direction = (playerTransform.position - agentTransform.position).normalized;
        var speed = 5f; // Adjust speed as needed
        agentTransform.position += direction * speed * Time.deltaTime;
        agentTransform.LookAt(playerTransform.position);
        // Check if the agent is close enough to the player
        if (Vector3.Distance(agentTransform.position, playerTransform.position) <= 5f)
        {
            // Stop moving when close enough
            return Status.Success;
        }
        // Continue moving towards the player
        return Status.Running;
    }
    
    protected override void OnEnd()
    {
    }
}

