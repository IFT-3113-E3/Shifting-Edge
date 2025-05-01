using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD.Health
{
    public class HealthBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Image borderImage;

        [Header("Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private Gradient healthGradient = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(Color.green, 0f),
                new(Color.yellow, 0.5f),
                new(Color.red, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new(1f, 0f),
                new(1f, 1f)
            }
        };

        private PlayerStats _stats;
        private float _targetFillAmount;
        private bool _isInitialized;

        private void Start()
        {
            if (GameManager.Instance.GameSession != null)
            {
                _stats = GameManager.Instance.GameSession.PlayerStats;
                if (_stats != null)
                {
                    Initialize(_stats);
                }
            }
            else
            {
                Debug.LogError("GameSession is not initialized.");
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

        private void UpdateHealth(int currentHealth)
        {
            if (_stats == null) return;

            _targetFillAmount = (float)currentHealth / _stats.maxHealth;
            healthFillImage.fillAmount = _targetFillAmount;
            healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);

            if (hideWhenFull && _targetFillAmount >= 1f)
            {
                borderImage.gameObject.SetActive(false);
            }
            else
            {
                borderImage.gameObject.SetActive(true);
            }
        }

        public void Initialize(PlayerStats stats)
        {
            _stats = stats;
            SetupFromStatus(stats);
        }

        private void SetupFromStatus(PlayerStats stats)
        {
            _targetFillAmount = (float)stats.currentHealth / stats.maxHealth;
            healthFillImage.fillAmount = _targetFillAmount;
            healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);

            stats.OnHealthChanged += UpdateHealth;
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnHealthChanged -= UpdateHealth;
            }
        }

        public void RefreshHealthBar()
        {
            if (_isInitialized)
            {
                _targetFillAmount = (float)_stats.currentHealth / _stats.maxHealth;
                healthFillImage.fillAmount = _targetFillAmount;
                healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);
            }
        }
    }
}
