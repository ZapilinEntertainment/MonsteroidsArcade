using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class InputManager : MonoBehaviour,IPausable
    {
        [SerializeField] private RectTransform _testObject;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private Vector3 _lastMousePosition;
        private bool _prepared = false, _keyboardOnly = false, _isPaused = false, _accelerating = false;
        
        public void Prepare(GameManager gm, PlayerController pc)
        {
            if (!_prepared)
            {
                _gameManager = gm;
                _playerController = pc;
                _isPaused = _gameManager.IsPaused;
                _keyboardOnly = GameConstants.IsControlKeyboardOnly();
                _gameManager.SubscribeToPauseEvent(this);
                _prepared = true;
            }
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
            _accelerating = false;
            if (x) _keyboardOnly = GameConstants.IsControlKeyboardOnly(); // управление могло поменяться во время паузы
            else {
                _lastMousePosition = Input.mousePosition;
                    }
        }

        private void Update()
        {
            if (!_prepared) return;
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape)) _gameManager.PauseSwitch();
                else
                {
                    if (!_isPaused)
                    {
                        bool newAcceleratingVal = false;
                        float x;
                        if (_keyboardOnly)
                        {
                            x = Input.GetAxis("Horizontal");
                            if (x != 0f) _playerController.Rotate(x * Time.deltaTime);
                            x = Input.GetAxisRaw("Vertical");
                            newAcceleratingVal = x != 0f;
                        }
                        else
                        {
                            if (Input.GetMouseButtonDown(1)) newAcceleratingVal = true;
                            var mpos = Input.mousePosition;
                            if (mpos != _lastMousePosition)
                            {
                                _playerController.RotateToPoint(mpos);
                                _lastMousePosition = mpos;
                                _testObject.position = _lastMousePosition;
                            }
                        }
                        if (newAcceleratingVal != _accelerating)
                        {
                            _accelerating = newAcceleratingVal;
                            _playerController.SwitchAccelerate(_accelerating);
                        }

                        if (Input.GetKeyDown(KeyCode.Space) || (!_keyboardOnly && Input.GetMouseButtonDown(0)) ) _playerController.Fire();                        
                    }
                }
            }
        }
    }
}
