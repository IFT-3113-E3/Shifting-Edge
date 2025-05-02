using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathSequence : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;
    public OrbitCamera orbitCamera;
    public GameObject gameOverUI;
    public CanvasGroup fadeGroup;
    public float fadeDuration = 2f;
    public string menuSceneName = "MainMenu";

    private bool isDead = false;

    private void Start()
    {
        
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        if (playerAnimator != null)
            playerAnimator.SetTrigger("Death");

        if (orbitCamera != null)
            orbitCamera.enabled = false;

        Time.timeScale = 0.5f;

        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        gameOverUI.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeGroup.alpha = 1f;

        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);

        bool anyKeyPressed = false;
        while (!anyKeyPressed)
        {
            if (Input.anyKeyDown)
            {
                anyKeyPressed = true;
                Time.timeScale = 1f;
                GameManager.Instance.ReturnToMainMenu();
            }
            yield return null;
        }
    }
}
