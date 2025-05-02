using UnityEngine;

public class NarrativeTrigger : MonoBehaviour
{
    public StoryEvent storyEvent;
    public bool triggerOnlyOnce = true;

    private bool _alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyTriggered && triggerOnlyOnce) return;
        if (!other.CompareTag("Player")) return;

        NarrativeManager.Instance.PlayEvent(storyEvent);
        _alreadyTriggered = true;
    }
}
