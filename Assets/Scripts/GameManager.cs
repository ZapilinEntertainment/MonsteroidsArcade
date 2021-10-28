using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MonsteroidsArcade
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;
        public GameManager Current { get; private set; }
        public PlayerController PlayerController { get; private set; }
        public MotionCalculator MotionCalculator { get; private set; }
        public GameSettings GameSettings => _gameSettings;
        private MainMenuUI _mainMenuUI;
        private Action<bool> _pauseEvent;
        private bool _gameStarted = false, _isPaused = true;
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
            // Space objects simulator
            MotionCalculator = GetComponent<MotionCalculator>();
            if (MotionCalculator == null) MotionCalculator = gameObject.AddComponent<MotionCalculator>();
            MotionCalculator.Prepare(this, PlayerController);
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
                _isPaused = false;
                _pauseEvent(false);
            }
        }

        public void PauseSwitch()
        {
            _isPaused = !_isPaused;
            _pauseEvent(IsPaused);
            _mainMenuUI.ActivateWindow(true);
        }
    }
}
