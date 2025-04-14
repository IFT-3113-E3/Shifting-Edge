using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public float shakeMultiplier = 1f;
    
    private Vector3 _originalLocalPosition;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        _originalLocalPosition = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 shakeOffset = Random.insideUnitSphere * (magnitude * shakeMultiplier * (1 - t));
            transform.localPosition = _originalLocalPosition + shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalLocalPosition;
        _shakeRoutine = null;
    }
}