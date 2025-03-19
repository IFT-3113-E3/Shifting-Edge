using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;

    private void Start()
    {
        string nextScene = SceneLoader.GetNextScene();
        if (!string.IsNullOrEmpty(nextScene))
        {
            StartCoroutine(LoadSceneAsync(nextScene));
        }
        else
        {
            Debug.LogError("Aucune scène cible spécifiée dans SceneLoader.");
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadingBar != null)
                loadingBar.value = progress;

            if (loadingText != null)
                loadingText.text = $"Chargement... {progress * 100:F0}%";

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
