using UnityEngine;

namespace MercsOfMayhem.Managers
{
    /// <summary>
    /// Main game manager - handles game state, score, lives, etc.
    /// Singleton pattern for easy access from anywhere.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private int playerLives = 3;
        [SerializeField] private int currentScore = 0;

        public int PlayerLives => playerLives;
        public int CurrentScore => currentScore;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // TODO: Initialize game systems
        }

        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged();
        }

        public void LoseLife()
        {
            playerLives--;
            OnLivesChanged();

            if (playerLives <= 0)
            {
                GameOver();
            }
        }

        private void OnScoreChanged()
        {
            // TODO: Update UI
        }

        private void OnLivesChanged()
        {
            // TODO: Update UI
        }

        private void GameOver()
        {
            Debug.Log("Game Over!");
            // TODO: Show game over screen
        }

        public void RestartGame()
        {
            playerLives = 3;
            currentScore = 0;
            // TODO: Reload scene
        }
    }
}
