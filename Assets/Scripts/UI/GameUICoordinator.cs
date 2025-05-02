using System;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// This class is responsible for managing the UI elements in the game, and hook them up to the game context.
    /// </summary>
    public class GameUICoordinator : MonoBehaviour
    {
        [SerializeField] private GameOverScreenView gameOverScreenView;
        [SerializeField] private PauseMenuManager pauseMenuManager;
        
        private void Awake()
        {
            // Hook up the game over screen to the game context
            var playerStats = GameManager.Instance.GameSession.PlayerStats;
            playerStats.OnDeath += HandleDeath;
            
            gameOverScreenView.OnAnyKeyPressedToContinue += HandleGameOverContinue;
        }
        
        private void HandleDeath()
        {
            pauseMenuManager.SetInputDisabled(true);
            gameOverScreenView.Show();
        }

        private void HandleGameOverContinue()
        {
            _ = GameManager.Instance.RestartAtLastCheckpoint();
            gameOverScreenView.Hide();
            pauseMenuManager.SetInputDisabled(false);
        }
        
        private void OnDestroy()
        {
            var playerStats = GameManager.Instance.GameSession.PlayerStats;
            playerStats.OnDeath -= HandleDeath;
        }
    }
}