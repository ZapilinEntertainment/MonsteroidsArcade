using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace MonsteroidsArcade
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;
        
        public static GameManager Current { get; private set; }
        public PlayerController PlayerController { get; private set; }
        public MotionCalculator MotionCalculator { get; private set; }
        public RectTransform GameZoneHost { get; private set; }
        public GameSettings GameSettings => _gameSettings;
        public int Score { get; private set; }

        private UIManager _uiManager;
        private Audiomaster _audiomaster;

        private Action<bool> _pauseEvent;
        private UFO _ufo;
        private bool _gameStarted = false, _isPaused = true,_waitForNextRound = false, _gameOver = false, _ufoLaunched = false;
        private int _asteroidsCount = 2;
        private float _nextRoundTimer, _ufoTimer;
        public bool IsPaused => _isPaused;

        private void Awake()
        {
            if (Current != null)
            {
                Destroy(this);
                return;
            }
            Current = this;           
            //
            if (_gameSettings == null)
            {
                Debug.Log("game settings undefined, switching to default");
                _gameSettings = new GameSettings();
            }                
            //
            PlayerController = FindObjectOfType<PlayerController>();
            if (PlayerController == null)
            {
                Debug.Log("Error - No player controller found");
                return;
            }
            PlayerController.Prepare(this);
            //
            _uiManager = FindObjectOfType<UIManager>();
            if (_uiManager == null)
            {
                Debug.Log("Error - UI Manager not found");
                return;
            }
            _uiManager.Prepare(this, PlayerController);
            _uiManager.ChangeStatus(GameUIStatus.MainMenu);
            GameZoneHost = _uiManager.GameZone;
            //
            MotionCalculator = GetComponent<MotionCalculator>();
            if (MotionCalculator == null) MotionCalculator = gameObject.AddComponent<MotionCalculator>();
            MotionCalculator.Prepare(this, PlayerController, GameZoneHost);
            PlayerController.AssignMotionCalculator(MotionCalculator);
            //
            _audiomaster = FindObjectOfType<Audiomaster>();
            if (_audiomaster == null)
            {
                _audiomaster = new GameObject("Audiomaster").AddComponent<Audiomaster>();
            }
            _audiomaster.Prepare();
        }

        public void SubscribeToPauseEvent(IPausable ip)
        {
            _pauseEvent += ip.SetPause;
        }
        // unsubscribe в данном случае не нужен, т.к. игровые объекты не удаляются

        public void StartNewSession()
        {
            if (_gameStarted)
            {
                MotionCalculator.DestroyAllObjects();
                _audiomaster.NewSessionStarted();
            }
            PlayerController.Spawn(true);
            _gameStarted = true;
            _gameOver = false;
            _waitForNextRound = false;
            _isPaused = false;
            _pauseEvent(false);
            //
            _asteroidsCount = _gameSettings.StartAsteroidsCount;
            MotionCalculator.CreateBigAsteroids(_asteroidsCount);
            _ufoTimer = _gameSettings.GetUfoTime();
            _ufoLaunched = false;
            Score = 0;
            //
            _uiManager.ChangeStatus(GameUIStatus.Playmode);
        }

        private void Update()
        {
            if (!IsPaused & !_gameOver)
            {
                float t = Time.deltaTime;
                if (_waitForNextRound)
                {
                    _nextRoundTimer -= t;
                    if (_nextRoundTimer <= 0f)
                    {
                        if (!_gameOver)
                        {
                            _asteroidsCount += _gameSettings.AsteroidsPerLevelSurplus;
                            MotionCalculator.CreateBigAsteroids(_asteroidsCount);                           
                        }
                        _waitForNextRound = false;
                    }
                }
                if (!_ufoLaunched)
                {
                    _ufoTimer -= t;
                    if (_ufoTimer <= 0f)
                    {
                        _ufo = MotionCalculator.LaunchUFO();
                        _ufo.Activate();
                        _ufoLaunched = true;
                    }
                }
            }
        }
        public void NextRound()
        {
            if (!_waitForNextRound & !_gameOver)
            {
                _waitForNextRound = true;
                _nextRoundTimer = _gameSettings.NextRoundDelay;
            }
        }
        public void UfoDestroyed()
        {
            if (_ufoLaunched)
            {
                _ufoLaunched = false;
                _ufoTimer = _gameSettings.GetUfoTime();
                Audiomaster.PlayEffect(AudioEffectType.UfoDefeated);
            }
        }
        public void AddScore(int val)
        {
            Score += val;
        }

        public void GameOver()
        {
            _gameOver = true;
            GameConstants.SetHighscore(Score);
            Audiomaster.PlayEffect(AudioEffectType.GameOver);            
            _uiManager.ChangeStatus(GameUIStatus.GameOver);            
        }

        public void PauseSwitch()
        {
            Audiomaster.PlayEffect(AudioEffectType.ButtonClicked);
            _isPaused = !_isPaused;
            _pauseEvent(IsPaused);
            _uiManager.ChangeStatus(IsPaused? GameUIStatus.PauseWindow : GameUIStatus.Playmode);
        }

        public void ReturnToMenu()
        {
            MotionCalculator.DestroyAllObjects();
            _uiManager.ChangeStatus(GameUIStatus.MainMenu);
        }
    }
}
