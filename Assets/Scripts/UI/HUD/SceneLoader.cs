using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public static class SceneLoader
{
    private static string nextScene;

    public static void LoadScene(string targetScene)
    {
        Debug.Log($"Chargement de la scène intermédiaire : vers {targetScene}");
        nextScene = targetScene;
        SceneManager.LoadScene("LoadingScreen");
    }

    public static string GetNextScene()
    {
        return nextScene;
    }
}
