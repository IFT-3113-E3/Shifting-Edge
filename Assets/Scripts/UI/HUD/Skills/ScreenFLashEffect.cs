using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlashEffect : MonoBehaviour
{
    [Header("Réglages")]
    public float flashDuration = 0.3f;
    
    [Header("Références UI")]
    public Image redFlashImage;
    public Image blueFlashImage;

    private Coroutine currentFlash;

    public void TriggerFlash(Color color)
    {
        Image targetImage = color == Color.red ? redFlashImage : blueFlashImage;
        
        if(currentFlash != null) 
            StopCoroutine(currentFlash);
        
        currentFlash = StartCoroutine(FlashRoutine(targetImage));
    }

    private IEnumerator FlashRoutine(Image flashImage)
    {
        Color originalColor = flashImage.color;
        flashImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while(elapsed < flashDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
        flashImage.color = originalColor;
    }
}