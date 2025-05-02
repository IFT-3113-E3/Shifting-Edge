using System;
using System.Collections;
using UnityEngine;

namespace UI
{

    public class GameOverScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject root;
        public CanvasGroup fadeGroup;
        public float fadeDuration = 2f;

        private Coroutine _fadeRoutine;
        private bool isShowing = false;
        
        public event Action OnAnyKeyPressedToContinue;


        public void Show()
        {
            if (isShowing) return;
            isShowing = true;
            root.SetActive(true);

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(FadeAndWaitForInput());
        }

        public void Hide()
        {
            if (!isShowing) return;
            isShowing = false;

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            Time.timeScale = 1f;
            fadeGroup.alpha = 0f;
            root.SetActive(false);
        }

        private IEnumerator FadeAndWaitForInput()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                Time.timeScale = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            fadeGroup.alpha = 1f;

            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(1f);

            while (!Input.anyKeyDown)
            {
                yield return null;
            }

            OnAnyKeyPressedToContinue?.Invoke();
        }
    }

}