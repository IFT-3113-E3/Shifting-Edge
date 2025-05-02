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
        [SerializeField] private HealthBarExt bossHealthBar;
        
        
        private void Awake()
        {
            // Hook up the game over screen to the game context
            var playerStats = GameManager.Instance.GameSession.PlayerStats;
            playerStats.OnDeath += HandleDeath;
            
            GameManager.Instance.GameSession.OnBossFightState += HandleBossFight;
            GameManager.Instance.GameSession.OnGameSessionResetTempData += OnGameSessionResetTempData;

            bossHealthBar.HideImmediately();
            
            gameOverScreenView.OnAnyKeyPressedToContinue += HandleGameOverContinue;
        }
        
        private void OnGameSessionResetTempData()
        {
            if (gameOverScreenView != null)
            {
                gameOverScreenView.Hide();
            }
            
            if (pauseMenuManager != null)
            {
                pauseMenuManager.SetInputDisabled(false);
            }
            
            if (bossHealthBar != null)
            {
                bossHealthBar.Unbind();
                bossHealthBar.HideImmediately();
            }
        }

        private void HandleBossFight(BossFightState bossFightState)
        {
            bossFightState.OnDefeated += () =>
            {
                bossHealthBar.Unbind();
                bossHealthBar.Hide();
            };
            Debug.Log("Boss fight state changed: " + bossFightState);
            bossHealthBar.Bind(bossFightState);
            bossHealthBar.Show();
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
            GameManager.Instance.GameSession.OnBossFightState -= HandleBossFight;
            GameManager.Instance.GameSession.OnGameSessionResetTempData -= OnGameSessionResetTempData;

            if (gameOverScreenView != null)
            {
                gameOverScreenView.OnAnyKeyPressedToContinue -= HandleGameOverContinue;
            }
            
            if (pauseMenuManager != null)
            {
                pauseMenuManager.SetInputDisabled(false);
            }
            
            if (bossHealthBar != null)
            {
                bossHealthBar.Unbind();
                bossHealthBar.HideImmediately();
            }
        }
    }
}