using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class InputManager : MonoBehaviour,IPausable
    {
        private GameManager _gameManager;
        private PlayerController _playerController;
        private Vector3 _lastMousePosition;
        private bool _prepared = false, _keyboardOnly = false, _isPaused = false, _accelerating = false;
        
        public void Prepare(GameManager gm, PlayerController pc)
        {
            if (!_prepared)
            {
                _gameManager = gm;
                _gameManager.SubscribeToPauseEvent(this);
                _isPaused = _gameManager.IsPaused;
                _playerController = pc;                
                _keyboardOnly = GameConstants.IsControlKeyboardOnly();                
                _prepared = true;
            }
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
            _accelerating = false;
            _keyboardOnly = GameConstants.IsControlKeyboardOnly(); // управление могло поменяться во время паузы
            _lastMousePosition = Input.mousePosition;
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
                        float x = Input.GetAxisRaw("Vertical");
                        newAcceleratingVal = x != 0f;
                        if (_keyboardOnly)
                        {
                            x = Input.GetAxis("Horizontal");
                            if (x != 0f) _playerController.Rotate(-x * Time.deltaTime);                            
                        }
                        else
                        {
                            if (!newAcceleratingVal) newAcceleratingVal = Input.GetMouseButton(1);
                            var mpos = Input.mousePosition;
                            if (mpos != _lastMousePosition)
                            {
                                _playerController.RotateToPoint(mpos);
                                _lastMousePosition = mpos;
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
