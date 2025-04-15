using UnityEngine;
using UnityEngine.UI;
using Status;

public class HealthBar : MonoBehaviour 
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image borderImage;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
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

    private EntityStatus _status;
    private float _targetFillAmount;
    private bool _isInitialized;

    private void Start()
    {
        if (_status == null)
        {
            GameObject owner = GameObject.FindGameObjectWithTag("Player");
            _status = owner.GetComponent<EntityStatus>();

            if (_status != null)
            {
                SetupFromStatus(_status);
            }
            else
            {
                Debug.LogError("HealthBar : Aucun EntityStatus trouvé.");
            }
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

    private void UpdateHealth(float damage, GameObject source)
    {
        if (!_isInitialized) return;

        _targetFillAmount = _status.CurrentHealth / _status.maxHealth;

        if (hideWhenFull)
        {
            gameObject.SetActive(!Mathf.Approximately(_targetFillAmount, 1f));
        }
    }

    public void Initialize(EntityStatus status)
    {
        _status = status;
        SetupFromStatus(status);
    }

    private void SetupFromStatus(EntityStatus status)
    {
        _targetFillAmount = status.CurrentHealth / status.maxHealth;
        healthFillImage.fillAmount = _targetFillAmount;
        healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);

        status.OnDamageTaken += UpdateHealth;
        _isInitialized = true;
    }

    private void OnDestroy()
    {
        if (_status != null)
            _status.OnDamageTaken -= UpdateHealth;
    }

    public void RefreshHealthBar()
    {
        if (_isInitialized)
        {
            _targetFillAmount = _status.CurrentHealth / _status.maxHealth;
            healthFillImage.fillAmount = _targetFillAmount;
            healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);
        }
    }
}
