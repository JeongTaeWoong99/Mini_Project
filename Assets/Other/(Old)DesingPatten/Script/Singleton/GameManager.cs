using System.Collections.Generic;
using DesingPatten.Observer;
using TMPro;
using UnityEngine;

namespace DesingPatten.Singleton
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        public float CurrentRemainTime { get; private set; }
        public TextMeshProUGUI remainGameText;
        private int min;
        private int sec;
        
        public int CurrentCoin { get; private set; }
        public TextMeshProUGUI coinText;
        public float coinGetTime = 2f;
        private float coinGetTimerCount;

        private bool isGameOver = false;

        private readonly Dictionary<GameEvent, List<IObserver>> eventObservers = new Dictionary<GameEvent, List<IObserver>>();

        public MonsterFactory monsterFactory;
        public Transform monsterSpawnTransList;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            CurrentRemainTime = 180f;
            CurrentCoin = 0;
            coinGetTimerCount = 0;
            InitializeEventObservers();
        }

        private void InitializeEventObservers()
        {
            foreach (GameEvent gameEvent in System.Enum.GetValues(typeof(GameEvent)))
            {
                eventObservers[gameEvent] = new List<IObserver>();
            }
        }

        private void Update()
        {
            if(isGameOver) return;
            
            UpdateRemainGameTime();
            UpdateCoinTime();
        }
        
        public void GetCoin(int getCoinNum)
        {
            CurrentCoin += getCoinNum;
            NotifyObservers(GameEvent.CoinChanged, CurrentCoin);
        }

        public bool HasCoin(int cost)
        {
            return CurrentCoin >= cost;
        }

        public void UseCoin(int cost)
        {
            if (CurrentCoin >= cost)
                CurrentCoin -= cost;
        }   

        private void UpdateRemainGameTime()
        {
            CurrentRemainTime -= Time.deltaTime;
            if(CurrentRemainTime <= 0f)
            {
                NotifyObservers(GameEvent.TimeUp);
                GameOver();
            }
            min = (int)CurrentRemainTime / 60;
            sec = (int)CurrentRemainTime % 60;
            if (remainGameText != null)
                remainGameText.text = string.Format("{0:D1}:{1:D2}", min, sec);
        }
        
        private void UpdateCoinTime()
        {
            if (coinGetTimerCount > 0)
                coinGetTimerCount -= Time.deltaTime;
            else
            {
                coinGetTimerCount = coinGetTime;
                GetCoin(1);
                SpawnMonster(); 
            }
            if (coinText != null)
                coinText.text = "$ : " + CurrentCoin;
        }

        public void GameOver()
        {
            if (isGameOver) return;
            
            isGameOver = true;
            NotifyObservers(GameEvent.CastleDestroyed);
            NotifyObservers(GameEvent.GameOver);
        }

        public void RegisterObserver(GameEvent gameEvent, IObserver observer)
        {
            if (eventObservers.TryGetValue(gameEvent, out List<IObserver> observers))
            {
                if (!observers.Contains(observer))
                {
                    observers.Add(observer);
                }
            }
        }
        
        public void UnregisterObserver(GameEvent gameEvent, IObserver observer)
        {
            if (eventObservers.TryGetValue(gameEvent, out List<IObserver> observers))
            {
                observers.Remove(observer);
            }
        }

        private void NotifyObservers(GameEvent gameEvent, object data = null)
        {
            if (eventObservers.TryGetValue(gameEvent, out List<IObserver> observers))
            {
                var observersCopy = new List<IObserver>(observers);
                foreach (var observer in observersCopy)
                {
                    if (observer != null)
                        observer.Notify(gameEvent, data);
                }
            }
        }

        public void SpawnMonster()
        {
            int random = Random.Range(0, 4);
            var monster = monsterFactory.Spawn((MonsterType)random, monsterSpawnTransList);
            if (monster != null)
                NotifyObservers(GameEvent.MonsterSpawned, monster);
        }
    }
}
