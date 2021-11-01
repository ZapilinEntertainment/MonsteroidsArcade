using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public sealed class PlayerController : SpaceObject, IPausable
    {
        [SerializeField] private ShipSettings _shipSettings;
        [SerializeField] private Transform _model;
        public bool IsInvincible { get; private set; }
        public bool IsDestroyed { get; private set; }
        public int LivesCount { get; private set; }
        private GameManager _gameManager;
        private MotionCalculator _motionCalculator;
        private Quaternion _rotation, _targetRotation;
        private InputManager _inputManager;
        private GameSettings _gameSettings;
        private Image _modelSprite;
        private bool _isPaused, _accelerate, _isRespawning;
        private float _stateChangeTimer = 0f, _flickerStartTime = 0f, _fireCooldownTimer = 0f;

        public void Prepare(GameManager gm)
        {
            _gameManager = gm;            
            _isPaused = _gameManager.IsPaused;
            _gameManager.SubscribeToPauseEvent(this);
            _gameSettings = _gameManager.GameSettings;
            LivesCount = _gameSettings.PlayerLivesCount;
            if (_shipSettings == null)
            {
                _shipSettings = new ShipSettings();
                Debug.Log("ship settings undefined, switching to default");
            }
            _inputManager = gameObject.GetComponent<InputManager>();
            if (_inputManager == null) _inputManager = gameObject.AddComponent<InputManager>();
            _inputManager.Prepare(_gameManager, this);
            //
            _modelSprite = _model.GetComponent<Image>();
        }
        public void AssignMotionCalculator(MotionCalculator mc)
        {
            _motionCalculator = mc;
        }


        public void Spawn()
        {
            Stop();
            transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            _rotation = Quaternion.identity;
            _model.rotation = _rotation;
            transform.rotation = _rotation;
            _targetRotation = _rotation;
            _stateChangeTimer = _gameManager.GameSettings.StartInvincibilityTime;
            IsInvincible = true;
            _fireCooldownTimer = 0f;
        }

        private void Update()
        {
            if (_isPaused) return;
            float t = Time.deltaTime;
            if (!IsDestroyed)
            {
                if (_rotation != _targetRotation)
                {
                    _rotation = Quaternion.RotateTowards(_rotation, _targetRotation, _shipSettings.RotationSpeed * t);
                    _model.rotation = _rotation;
                }
                if (_accelerate)
                {
                    _moveVector = Vector3.MoveTowards(_moveVector, (_rotation * Vector3.up) * _shipSettings.MaxSpeed, _shipSettings.Acceleration * t);
                    //_moveVector = (_rotation * Vector3.up) * _shipSettings.MaxSpeed;
                }
                if (IsInvincible)
                {
                    _stateChangeTimer -= t;
                    if (_stateChangeTimer <= 0f)
                    {
                        _stateChangeTimer = 0f;
                        IsInvincible = false;
                        _modelSprite.color = Color.white;
                    }
                    else
                    {
                        _modelSprite.color = new Color(1f, 1f, 1f, 1f - Mathf.PingPong((Time.time - _flickerStartTime) / _shipSettings.InvincibilityFlickeringTime, 1f));
                    }
                }
                if (_fireCooldownTimer > 0f)
                {
                    _fireCooldownTimer -= t;
                    if (_fireCooldownTimer < 0f) _fireCooldownTimer = 0f;
                } 
            }
            else
            {
                if (_isRespawning)
                {
                    _stateChangeTimer -= t;
                    if (_stateChangeTimer <= 0f)
                    {
                        _stateChangeTimer = _gameSettings.StartInvincibilityTime;
                        IsInvincible = true;
                        _flickerStartTime = Time.time;
                        IsDestroyed = false;
                        _isRespawning = false;
                        _model.gameObject.SetActive(true); // + effect
                    }
                    
                }
            }
        }

        public void Fire()
        {

            if (_isPaused || _fireCooldownTimer != 0f) return;
            else
            {
                Vector3 d = _rotation * Vector3.up;
                _motionCalculator.CreateBullet(transform.position + d * Radius, d, true);
                _fireCooldownTimer = _gameSettings.PlayerFireCooldown;
            }
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

        public void MakeDestroyed()
        {
            IsDestroyed = true;
            Stop();
            LivesCount--;
            _model.gameObject.SetActive(false);
            if (LivesCount <= 0)
            {
                _gameManager.GameOver();
                _isRespawning = false;
            }
            else
            {
                _stateChangeTimer = _gameSettings.RespawnDelay;
                _isRespawning = true;
            }
        }

    }
}
