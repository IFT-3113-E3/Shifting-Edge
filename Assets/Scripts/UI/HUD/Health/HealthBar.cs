using UnityEngine;
using UnityEngine.UI;
using Status;

public class HealthBar : MonoBehaviour 
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage; // Doit être en mode Filled/Horizontal/Left
    [SerializeField] private Image borderImage;
    
    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private Gradient healthGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.green, 0f),  // Vert quand la vie est pleine
            new GradientColorKey(Color.yellow, 0.5f),
            new GradientColorKey(Color.red, 1f)     // Rouge quand la vie est vide
        },
        alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        }
    };

    private EntityStatus _playerStatus;
    private float _targetFillAmount;
    private bool _isInitialized;

    private void Start()
    {
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player non trouvé - vérifiez le tag 'Player'");
            return;
        }

        _playerStatus = player.GetComponent<EntityStatus>();
        if (_playerStatus == null)
        {
            Debug.LogError("EntityStatus non trouvé sur le Player");
            return;
        }

        // Initialisation synchrone
        _targetFillAmount = _playerStatus.CurrentHealth / _playerStatus.maxHealth;
        healthFillImage.fillAmount = _targetFillAmount;
        healthFillImage.color = healthGradient.Evaluate(_targetFillAmount); // Pas d'inversion ici

        _playerStatus.OnDamageTaken += UpdateHealth;
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // Animation fluide
        if (Mathf.Abs(healthFillImage.fillAmount - _targetFillAmount) > 0.001f)
        {
            healthFillImage.fillAmount = Mathf.Lerp(
                healthFillImage.fillAmount, 
                _targetFillAmount, 
                smoothSpeed * Time.deltaTime
            );
            
            // Application directe du gradient
            healthFillImage.color = healthGradient.Evaluate(healthFillImage.fillAmount);
        }
    }

    private void UpdateHealth(DamageRequest damageRequest)
    {
        if (!_isInitialized) return;
        
        _targetFillAmount = _playerStatus.CurrentHealth / _playerStatus.maxHealth;
        
        if (hideWhenFull)
        {
            gameObject.SetActive(!Mathf.Approximately(_targetFillAmount, 1f));
        }
    }

    private void OnDestroy()
    {
        if (_playerStatus != null)
            _playerStatus.OnDamageTaken -= UpdateHealth;
    }

    // Méthode pour forcer une mise à jour (utile si maxHealth change)
    public void RefreshHealthBar()
    {
        if (_isInitialized)
        {
            _targetFillAmount = _playerStatus.CurrentHealth / _playerStatus.maxHealth;
            healthFillImage.fillAmount = _targetFillAmount;
            healthFillImage.color = healthGradient.Evaluate(_targetFillAmount);
        }
    }
}