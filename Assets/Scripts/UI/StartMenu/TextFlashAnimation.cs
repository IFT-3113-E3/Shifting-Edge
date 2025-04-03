using System.Collections;
using UnityEngine;
using TMPro;

public class TextFlashAnimation : MonoBehaviour
{
    public float flashInterval = 0.8f;
    private TextMeshProUGUI textMeshPro;

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshProUGUI non trouvé sur le GameObject.");
            return;
        }

        StartCoroutine(FlashTextCoroutine());
    }

    private IEnumerator FlashTextCoroutine()
    {
        while (true)
        {
            textMeshPro.alpha = 0;
            yield return new WaitForSeconds(flashInterval);

            textMeshPro.alpha = 1;
            yield return new WaitForSeconds(flashInterval);
        }
    }
}
