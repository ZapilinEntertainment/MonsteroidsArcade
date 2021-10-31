using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private RectTransform _gameZoneHost;
        public static GameManager Current { get; private set; }
        public PlayerController PlayerController { get; private set; }
        public MotionCalculator MotionCalculator { get; private set; }
        public RectTransform GameZoneHost => _gameZoneHost;
        public GameSettings GameSettings => _gameSettings;
        private MainMenuUI _mainMenuUI;
        private Action<bool> _pauseEvent;
        private bool _gameStarted = false, _isPaused = true,_waitForNextRound = false, _gameOver = false;
        private int _asteroidsCount = 2;
        private float _nextRoundTimer;
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
            _mainMenuUI = FindObjectOfType<MainMenuUI>();
            if (_mainMenuUI == null)
            {
                Debug.Log("Error - No main menu script found");
                return;
            }
            _mainMenuUI.Prepare(this);
            //
            PlayerController = FindObjectOfType<PlayerController>();
            if (PlayerController == null)
            {
                Debug.Log("Error - No player controller found");
                return;
            }
            PlayerController.Prepare(this);
            //
            MotionCalculator = GetComponent<MotionCalculator>();
            if (MotionCalculator == null) MotionCalculator = gameObject.AddComponent<MotionCalculator>();
            MotionCalculator.Prepare(this, PlayerController);
            PlayerController.AssignMotionCalculator(MotionCalculator);
            //
            _mainMenuUI.ActivateWindow(false);
        }

        public void SubscribeToPauseEvent(IPausable ip)
        {
            _pauseEvent += ip.SetPause;
        }
        // unsubscribe в данном случае не нужен, т.к. игровые объекты не удаляются

        public void StartNewSession()
        {
            if (!_gameStarted)
            {
                PlayerController.Spawn();
                _gameStarted = true;
                _gameOver = false;
                _waitForNextRound = false;
                _isPaused = false;
                _pauseEvent(false);
                //
                _asteroidsCount = _gameSettings.StartAsteroidsCount;
                MotionCalculator.CreateBigAsteroids(_asteroidsCount);
            }
        }

        private void Update()
        {
            if (!IsPaused)
            {
                if (_waitForNextRound)
                {
                    _nextRoundTimer -= Time.deltaTime;
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
            }
        }
        public void NextRound()
        {
            if (!_waitForNextRound)
            {
                _waitForNextRound = true;
                _nextRoundTimer = _gameSettings.NextRoundDelay;
            }
        }

        public void GameOver()
        {
            _gameOver = true;
            Debug.Log("game over");
        }

        public void PauseSwitch()
        {
            _isPaused = !_isPaused;
            _pauseEvent(IsPaused);
            _mainMenuUI.ActivateWindow(true);
        }
    }
}
