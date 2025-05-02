using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Status;
using TMPro;

public class HealthBarExt : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Settings")] [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool hideWhenFull = true;

    [SerializeField] private Gradient healthGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.green, 0f),
            new GradientColorKey(Color.yellow, 0.5f),
            new GradientColorKey(Color.red, 1f)
        },
        alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        }
    };

    private float _targetFillAmount;
    private bool _isInitialized;
    private Coroutine _fadeCoroutine;

    private BossFightState _bossFightState;

    public void Bind(BossFightState bossFightState)
    {
        _bossFightState = bossFightState;
        if (_bossFightState != null)
        {
            SetupFromStatus(_bossFightState);
        }
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (Mathf.Abs(healthFillImage.fillAmount - _targetFillAmount) > 0.001f)
        {
            healthFillImage.fillAmount = Mathf.Lerp(
                healthFillImage.fillAmount,
                _targetFillAmount,
                smoothSpeed * Time.deltaTime
            );
            healthFillImage.color = healthGradient.Evaluate(healthFillImage.fillAmount);
        }
    }

    private void UpdateHealth()
    {
        if (!_isInitialized) return;

        _targetFillAmount = (float)_bossFightState.Health / _bossFightState.MaxHealth;

        if (hideWhenFull)
        {
            gameObject.SetActive(!Mathf.Approximately(_targetFillAmount, 1f));
        }
    }


    private void SetupFromStatus(BossFightState status)
    {
        _targetFillAmount = (float)status.Health / status.MaxHealth;
        healthFillImage.fillAmount = _targetFillAmount;
        healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);
        nameText.text = status.BossType;

        status.OnHealthChanged += OnDamageTaken;
        _isInitialized = true;
    }

    public void Unbind()
    {
        if (_bossFightState != null)
        {
            _bossFightState.OnHealthChanged -= OnDamageTaken;
        }

        _targetFillAmount = 0f;
        healthFillImage.fillAmount = _targetFillAmount;
        healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);
        _isInitialized = false;
    }

    private void OnDamageTaken(int health)
    {
        UpdateHealth();
    }

    public void Show()
    {
        if (!_isInitialized) return;
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        if (!_isInitialized) return;
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void ShowImmediately()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        canvasGroup.alpha = 1f;
    }

    public void HideImmediately()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        canvasGroup.alpha = 0f;
    }
}