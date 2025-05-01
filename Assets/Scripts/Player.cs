using System;
using Status;
using UnityEngine;

/**
 * A player class that gathers necessary components for a player asset.
 */
public class Player : MonoBehaviour
{
    private GameObject _playerGameObject;
    private EntityStatus _playerStatus;
    private PlayerController _playerController;
    private EntityMovementController _playerMovementController;
    
    // Model objects
    private PlayerStats _playerStats;
    
    public GameObject GameObject => _playerGameObject;
    public EntityStatus Status => _playerStatus;
    public EntityMovementController MovementController => _playerMovementController;
    public PlayerController Controller => _playerController;

    public event Action OnDeath;
    
    private void Awake()
    {
        _playerGameObject = gameObject;
        _playerStatus = GetComponent<EntityStatus>();
        _playerController = GetComponent<PlayerController>();
        _playerMovementController = GetComponent<EntityMovementController>();
        if (_playerStatus == null)
        {
            Debug.LogError("PlayerStatus component not found on player object.");
        }
        if (_playerController == null)
        {
            Debug.LogError("PlayerController component not found on player object.");
        }
        if (_playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController component not found on player object.");
        }
    }

    public void Bind(PlayerStats playerStats)
    {
        _playerStatus.SetMaxHealth(playerStats.maxHealth);
        _playerStatus.SetCurrentHealth(playerStats.currentHealth);
        
        _playerStats = playerStats;
        
        _playerStatus.OnHealthChanged += UpdateHealth;
        _playerStatus.OnDeath += OnStatusDeath;
        _playerController.OnAttack += _playerStats.SetAbilityStacks;
    }
    
    public void Unbind()
    {
        if (_playerStatus != null)
        {
            _playerStatus.OnHealthChanged -= UpdateHealth;
            _playerStatus.OnDeath -= OnStatusDeath;
            _playerStats = null;
        }
    }

    private void OnDisable()
    {
        if (_playerStatus != null)
        {
            Unbind();
        }
    }

    private void UpdateHealth(float currentHealth)
    {
        _playerStats.SetHealth((int)currentHealth);
    }
    
    private void OnAttack(int comboIndex)
    {
        _playerStats.SetAbilityStacks(_playerStats.abilityStacks + 1);
    }
    
    private void OnStatusDeath(DamageRequest damageRequest)
    {
        _playerStats.Die();
        OnDeath?.Invoke();
    }
}