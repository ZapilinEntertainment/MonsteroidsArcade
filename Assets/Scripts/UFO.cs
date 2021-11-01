using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class UFO : SpaceObject,IPausable
    {
        override public Vector3 MoveVector => _moveVector * _screenWidth;
        private GameSettings _gameSettings;
        private MotionCalculator _motionCalculator;
        private Transform _playerTransform;
        private float _screenWidth = Screen.width, _shootTimer = 0f;
        private bool  _isPaused = false;


        public void AssignLinks(GameSettings i_gs, MotionCalculator i_mc, Transform i_playerTransform)
        {
            _gameSettings = i_gs;
            _motionCalculator = i_mc;
            _playerTransform = i_playerTransform;
        }
        public void Activate()
        {
            _shootTimer = _gameSettings.GetUfoFireCooldown();
        }
        private void Update()
        {
            if (!_isPaused)
            {
                _shootTimer -= Time.deltaTime;
                if (_shootTimer <= 0f)
                {
                    _shootTimer = _gameSettings.GetUfoFireCooldown();
                    Vector3 pos = transform.position;
                    _motionCalculator.CreateBullet(pos, (_playerTransform.position - pos).normalized, false);
                }
            }
        }
        public override void ChangeCanvasScale(float x)
        {
            base.ChangeCanvasScale(x);
            _screenWidth = Screen.width;
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
        }
    }
}
