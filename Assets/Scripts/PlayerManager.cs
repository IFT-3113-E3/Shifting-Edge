using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    
    private Player _playerPrefab;
    private Player _playerInstance;
    private GameSession _gameSession;

    public Player Player => _playerInstance;
    
    public void Initialize(GameSession gameSession, Player playerPrefab)
    {
        _playerPrefab = playerPrefab;
        _gameSession = gameSession;
    }

    public void SpawnAt(Vector3 position, Quaternion rotation)
    {
        Debug.Log("Spawning player at: " + position);
        if (!_playerInstance)
        {
            Debug.Log("Instantiating player prefab.");
            _playerInstance = Instantiate(_playerPrefab, position, rotation);
        }
        _playerInstance.MovementController.SetPosition(position);
        _playerInstance.MovementController.SetRotation(rotation);

        _playerInstance.Bind(_gameSession.PlayerStats);
        _playerInstance.OnDeath += HandlePlayerDeath;
        
        EnableInput(true);
    }
    
    public void Despawn()
    {
        if (!_playerInstance) return;
        _playerInstance.OnDeath -= HandlePlayerDeath;
        _playerInstance.Unbind();
        Destroy(_playerInstance.gameObject);
        _playerInstance = null;
    }

    public void Kill()
    {
        EnableInput(false);
    }

    public void EnableInput(bool inputEnabled)
    {
        if (_playerInstance && _playerInstance.TryGetComponent(out PlayerController controller))
        {
            controller.SetInputEnabled(inputEnabled);
        }
    }
    
    private void HandlePlayerDeath()
    {
        // Handle player death logic here
        Debug.Log("Player has died.");
        EnableInput(false);
    }
}