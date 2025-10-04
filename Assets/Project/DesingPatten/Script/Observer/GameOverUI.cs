using DesingPatten.Singleton;
using UnityEngine;

namespace DesingPatten.Observer
{
    public class GameOverUI : MonoBehaviour, IObserver
    {
        public GameObject gameOverPanel;
        public GameObject timeUpPanel;
        
        private void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (timeUpPanel != null)
                timeUpPanel.SetActive(false);
            
            GameManager.Instance.RegisterObserver(GameEvent.GameOver, this);
            GameManager.Instance.RegisterObserver(GameEvent.TimeUp, this);
        }

        public void Notify(GameEvent gameEvent, object data = null)
        {
            switch (gameEvent)
            {
                case GameEvent.GameOver:
                    if (gameOverPanel != null)
                        gameOverPanel.SetActive(true);
                    break;
                case GameEvent.TimeUp:
                    if (timeUpPanel != null)
                        timeUpPanel.SetActive(true);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterObserver(GameEvent.GameOver, this);
                GameManager.Instance.UnregisterObserver(GameEvent.TimeUp, this);
            }
        }
    }
}

