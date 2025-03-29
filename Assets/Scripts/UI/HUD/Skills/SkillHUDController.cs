using UnityEngine;
using UnityEngine.UI;

public class SkillHUDController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] stackImages;
    [SerializeField] private float fillSpeed = 5f;

    public void Initialize(int maxStacks)
    {
        foreach (var img in stackImages)
        {
            img.fillAmount = 0;
        }
    }

    public void UpdateStacks(int currentStacks)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateStacks(currentStacks));
    }

    private System.Collections.IEnumerator AnimateStacks(int targetStacks)
    {
        float[] targetFills = new float[stackImages.Length];
        for (int i = 0; i < targetFills.Length; i++)
        {
            targetFills[i] = (i < targetStacks) ? 1f : 0f;
        }

        while (!AllFillsMatch(targetFills))
        {
            for (int i = 0; i < stackImages.Length; i++)
            {
                stackImages[i].fillAmount = Mathf.MoveTowards(
                    stackImages[i].fillAmount,
                    targetFills[i],
                    fillSpeed * Time.deltaTime
                );
            }
            yield return null;
        }
    }

    private bool AllFillsMatch(float[] targets)
    {
        for (int i = 0; i < stackImages.Length; i++)
        {
            if (Mathf.Abs(stackImages[i].fillAmount - targets[i]) > 0.01f)
                return false;
        }
        return true;
    }
}