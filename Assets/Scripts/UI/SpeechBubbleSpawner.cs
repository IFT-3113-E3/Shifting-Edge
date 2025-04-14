using UnityEngine;

namespace UI
{
    public class SpeechBubbleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject speechBubblePrefab;
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

        private SpeechBubbleController speechBubble;

        void Start()
        {
            var go = Instantiate(speechBubblePrefab, transform);
            speechBubble = go.GetComponent<SpeechBubbleController>();
            go.SetActive(false);
        }

        public void Speak(string message, float duration = 2f, Transform target = null)
        {
            speechBubble.SetTarget(target);
            speechBubble.ShowText(message, duration);
        }
    }

}