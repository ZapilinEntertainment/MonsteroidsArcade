using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class PlayerController : SpaceObject, IPausable
    {
        [SerializeField] private ShipSettings _shipSettings;
        [SerializeField] private Transform _sprite;
        public bool IsInvincible { get; private set; }
        public bool IsDestroyed { get; private set; }
        private GameManager _gameManager;
        private Quaternion _rotation, _targetRotation;
        private InputManager _inputManager;
        private bool _isPaused, _accelerate;

        public void Prepare(GameManager gm)
        {
            _gameManager = gm;
            _isPaused = _gameManager.IsPaused;
            _gameManager.SubscribeToPauseEvent(this);
            if (_shipSettings == null)
            {
                _shipSettings = new ShipSettings();
                Debug.Log("ship settings undefined, switching to default");
            }
            _inputManager = gameObject.GetComponent<InputManager>();
            if (_inputManager == null) _inputManager = gameObject.AddComponent<InputManager>();
            _inputManager.Prepare(_gameManager, this);
        }

        public void Spawn()
        {
            transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            _rotation = Quaternion.identity;
            _sprite.rotation = _rotation;
            transform.rotation = _rotation;
            _targetRotation = _rotation;
        }

        private void Update()
        {
            if (_isPaused) return;
            float t = Time.deltaTime;
            if (_rotation != _targetRotation)
            {
                _rotation = Quaternion.RotateTowards(_rotation, _targetRotation, _shipSettings.RotationSpeed * t);
                _sprite.rotation = _rotation;
            }
            if (_accelerate)
            {
                MoveVector = Vector3.MoveTowards(MoveVector, (_rotation * Vector3.up) * _shipSettings.MaxSpeed, _shipSettings.Acceleration * t);
                MoveVector = (_rotation * Vector3.up) * _shipSettings.MaxSpeed;
            }
        }

        public void Fire()
        {
            if (!_isPaused) return;
        }
        public void RotateToPoint(Vector3 point)
        {
            Vector3 dir = point - transform.position;
            if (dir.magnitude != 0f)
            {
                _targetRotation = Quaternion.LookRotation(Vector3.forward, dir.normalized);
            }
        }
        public void Rotate(float x)
        {
            _targetRotation *= Quaternion.Euler(0f, 0f, _shipSettings.RotationSpeed * x);
        }
        public void SwitchAccelerate(bool x)
        {
            _accelerate = x;
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
            _accelerate = false;
        }

    }
}
