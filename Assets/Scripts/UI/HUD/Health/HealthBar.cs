using UnityEngine;
using UnityEngine.UI;
using Status;

public class HealthBar : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image borderImage;
    
    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool hideWhenFull = true;

    private EntityStatus _targetStatus;
    private float _targetFill;
    private RectTransform _fillRect;

    private void Awake() 
    {
        // Trouve automatiquement EntityStatus sur le parent
        _targetStatus = GetComponentInParent<EntityStatus>();
        _fillRect = fillImage.GetComponent<RectTransform>();
        
        if (_targetStatus == null) 
        {
            Debug.LogError($"HealthBar sur {gameObject.name} : Aucun EntityStatus trouvé sur le parent !");
            enabled = false;
            return;
        }
    }

    private void OnEnable() => _targetStatus.OnDamageTaken += UpdateHealth;
    private void OnDisable() => _targetStatus.OnDamageTaken -= UpdateHealth;

    private void Update() 
    {
        // Animation fluide
        float currentFill = _fillRect.localScale.x;
        if (Mathf.Abs(currentFill - _targetFill) > 0.01f) 
        {
            float newFill = Mathf.Lerp(currentFill, _targetFill, smoothSpeed * Time.deltaTime);
            _fillRect.localScale = new Vector3(newFill, 1, 1);
        }
    }

    private void UpdateHealth(float damage, GameObject source) 
    {
        _targetFill = Mathf.Clamp01(_targetStatus.CurrentHealth / _targetStatus.maxHealth);
        
        if (hideWhenFull)
            gameObject.SetActive(!Mathf.Approximately(_targetFill, 1f));
    }
}