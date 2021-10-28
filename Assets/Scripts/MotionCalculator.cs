using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade {
    public sealed class MotionCalculator : MonoBehaviour,IPausable
    {
        [SerializeField] private float _simulationTick = 0.01f;
        private bool _prepared = false, _isPaused = false, _tick1 = false;
        private int _fastObjectsCount = 0, _slowObjectsCount = 0, _fastObjectsMask = 0;
        private float _screenWidth, _screenHeight;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private Transform _playerTransform;
        private List<SpaceObject> _fastObjects, _slowObjects;

        public void Prepare(GameManager gm, PlayerController pc)
        {
            _gameManager = gm;
            _isPaused = _gameManager.IsPaused;
            _gameManager.SubscribeToPauseEvent(this);

            _playerController = pc;
            _playerTransform = pc.transform;
            Time.fixedDeltaTime = _simulationTick;
            _prepared = true;
            
            _fastObjects = new List<SpaceObject>();
            _slowObjects = new List<SpaceObject>();
            //
            _fastObjectsMask = 0;
            for (int i = 0; i < (int)SpaceObjectType.Total; i++)
            {
                if (((SpaceObjectType)i).IsFastObject())
                {
                    _fastObjectsMask += (1 << i);
                }
            }
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
        }
        
        public void AddObjectToSimulation(SpaceObject so)
        {
            bool fastObject = ((1 << ((int)so.ObjectType)) & _fastObjectsMask) != 0;
            if (fastObject)
            {
                _fastObjects.Add(so);
                _fastObjectsCount++;
            }
            else
            {
                _slowObjects.Add(so);
                _slowObjectsCount++;
            }
        }

        private void FixedUpdate()
        {
            if (_prepared & !_isPaused)
            {
                bool calculatePlayer = !_playerController.IsInvincible;
                Vector3 playerPos = _playerTransform.position + _playerController.MoveVector * _simulationTick;               
                _playerTransform.position= CheckPosition(playerPos);

                if (calculatePlayer)
                {                   
                    // симуляция с учетом корабля игрока
                    if (_fastObjectsCount > 0)
                    {
                        foreach (SpaceObject so in _fastObjects)
                        {
                            so.transform.position += so.MoveVector * _simulationTick;
                        }
                    }
                    if (_tick1)
                    {
                        if (_slowObjectsCount > 0)
                        {
                            foreach (SpaceObject so in _fastObjects)
                            {
                                so.transform.position += so.MoveVector * _simulationTick * 2f;
                            }
                        }
                    }
                }
                _tick1 = !_tick1;

                Vector3 CheckPosition( Vector3 pos)
                {
                    if (pos.x < 0f) pos.x = _screenWidth + pos.x;
                    else if (pos.x > _screenWidth) pos.x -= _screenWidth;
                    if (pos.y < 0f) pos.y = _screenHeight + pos.y;
                    else if (pos.y > _screenHeight) pos.y -= _screenHeight;
                    return pos;
                }
            }
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
        }
    }
}
