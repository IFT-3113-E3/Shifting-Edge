using System;
using System.Collections;
using UnityEngine;
using TMPro;


namespace UI
{
    public class SpeechBubbleController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float duration = 2f;

        private Coroutine _currentRoutine;
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void ShowText(string message, float? customDuration = null)
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);

            text.text = message;
            gameObject.SetActive(true);
            _currentRoutine = StartCoroutine(HideAfterDelay(customDuration ?? duration));
        }

        private void LateUpdate()
        {
            if (Camera.main)
            {
                transform.forward = Camera.main.transform.forward;
            }
            if (target)
            {
                transform.position = target.position + new Vector3(0, 5, 0);
            }
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }
    }

}