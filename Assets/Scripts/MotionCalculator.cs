using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade {
    public enum CalculatingType : byte { Player = 0, UFO, Asteroids, PlayerBullet, UfoBullet, Total }
    public sealed class MotionCalculator : MonoBehaviour,IPausable
    {       
        // Рассчитывает передвижение и коллизию всех объектов.
        [SerializeField] private float _simulationTick = 1f / 120f;
        private BitArray _presentedObjectsMask;
        private bool _prepared = false, _isPaused = false, _needToClear = false;
        private SpaceObjectType _asteroidsMask, _playerKillerMask, _ufoKillerMask, _playerBulletTargetMask, _ufoBulletTargetMask;
        private float _screenWidth, _screenHeight;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private UFO _ufo;
        private Transform _playerTransform, _ufoTransform;
        private Dictionary<SpaceObject, Transform> _asteroidsList, _playerBulletsList, _ufoBulletsList;
        private SpaceObject _playerBulletToDelete, _ufoBulletToDelete; // пули, вылетевшие за край экрана;две пули оказавшиеся вне экрана в один фрейм - крайне редкий сценарий
        private HashSet<SpaceObject> _playerBulletsClearList, _asteroidsClearList; // список для очищения. Hashset так как не допускает повторений и порядок неважен 
        private PoolManager _poolManager;
        public void Prepare(GameManager gm, PlayerController pc)
        {
            _gameManager = gm;
            _isPaused = _gameManager.IsPaused;
            _gameManager.SubscribeToPauseEvent(this);

            _playerController = pc;
            _playerTransform = pc.transform;
            Time.fixedDeltaTime = _simulationTick;            
            //
            _asteroidsMask = SpaceObjectType.SmallAsteroid | SpaceObjectType.MediumAsteroid | SpaceObjectType.BigAsteroid;
            _playerKillerMask = _asteroidsMask | SpaceObjectType.UFOBullet;
            _ufoKillerMask = _asteroidsMask | SpaceObjectType.UFOBullet;
            _playerBulletTargetMask = _asteroidsMask | SpaceObjectType.UFO;
            _ufoBulletTargetMask =  SpaceObjectType.Player;
            //
            _asteroidsList = new Dictionary<SpaceObject, Transform>();
            _playerBulletsList = new Dictionary<SpaceObject, Transform>();
            _ufoBulletsList = new Dictionary<SpaceObject, Transform>();
            _presentedObjectsMask = new BitArray((int)CalculatingType.Total, false);
            _playerBulletsClearList = new HashSet<SpaceObject>();
            _asteroidsClearList = new HashSet<SpaceObject>();
            //           
            _poolManager = new PoolManager(_gameManager.GameZoneHost);
            //
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            //
            _prepared = true;
        }
        
        public void AddObjectToSimulation(SpaceObject so)
        {
            var type = so.ObjectType;
            if ((type & _asteroidsMask) != 0) _asteroidsList.Add(so, so.transform);
            else
            {
                if (type == SpaceObjectType.PlayerBullet)
                {
                    _playerBulletsList.Add(so, so.transform); 
                }
                else
                {
                    if (type == SpaceObjectType.UFOBullet)
                    {
                        _playerBulletsList.Add(so, so.transform);
                    }
                    else Debug.Log("warning - wrong space object type tried to enlist");
                }
            }
        }
        public void CreateObject(SpaceObjectType type, Vector3 position)
        {
            SpaceObject g = _poolManager.CreateObject(type, position); 
            if ((type & _asteroidsMask) != 0)
            {
                _asteroidsList.Add(g, g.transform);
                _presentedObjectsMask[(int)CalculatingType.Asteroids] = true;
            }
            else
            {
                if (type == SpaceObjectType.PlayerBullet)
                {
                    _playerBulletsList.Add(g, g.transform);
                    _presentedObjectsMask[(int)CalculatingType.PlayerBullet] = true;
                }
                else
                {
                    if (type == SpaceObjectType.UFOBullet)
                    {
                        _ufoBulletsList.Add(g, g.transform);
                        _presentedObjectsMask[(int)CalculatingType.UfoBullet] = true;
                    } 
                    // в ином случае будет вызван null
                }
            }
        }

        private void FixedUpdate()
        {
            
            if (_prepared & !_isPaused)
            {
                Vector3 pos0;
                Transform t;
                // MOVEMENT
                if (ObjectsPresented(CalculatingType.Player))
                {
                    _playerTransform.position = CheckPosition(_playerTransform.position + _playerController.MoveVector * _simulationTick);
                }
                if (ObjectsPresented(CalculatingType.UFO))
                {
                    _ufoTransform.position = CheckPosition(_ufoTransform.position + _ufo.MoveVector * _simulationTick);
                }
                //
                if (ObjectsPresented(CalculatingType.PlayerBullet))
                {
                    foreach (var a in _playerBulletsList)
                    {
                        t = a.Value;
                        pos0 = t.position + a.Key.MoveVector * _simulationTick;
                        t.position = pos0;
                        if (IsOutOfTheScreen(pos0)) _playerBulletToDelete = a.Key;
                    }
                }
                if (_playerBulletToDelete != null)
                {
                    DestroyPlayerBullet(_playerBulletToDelete);
                    _playerBulletToDelete = null;
                }
                //
                if (ObjectsPresented(CalculatingType.UfoBullet))
                {
                    foreach (var a in _ufoBulletsList)
                    {
                        t = a.Value;
                        pos0 = t.position + a.Key.MoveVector * _simulationTick;
                        t.position = pos0;
                        if (IsOutOfTheScreen(pos0)) _ufoBulletToDelete = a.Key;
                    }
                }
                if (_ufoBulletToDelete != null)
                {
                    DestroyUfoBullet(_ufoBulletToDelete);
                    _ufoBulletToDelete = null;
                }
                //
                if (ObjectsPresented(CalculatingType.Asteroids))
                {
                    foreach (var a in _asteroidsList) a.Value.position = CheckPosition(a.Key.MoveVector * _simulationTick);
                }

                

                Vector3 CheckPosition(Vector3 pos)
                {
                    if (pos.x < 0f) pos.x = _screenWidth + pos.x;
                    else if (pos.x > _screenWidth) pos.x -= _screenWidth;
                    if (pos.y < 0f) pos.y = _screenHeight + pos.y;
                    else if (pos.y > _screenHeight) pos.y -= _screenHeight;
                    return pos;
                }
                bool IsOutOfTheScreen(in Vector3 pos)
                {
                    return (pos.x < 0f || pos.y < 0f || pos.x > _screenWidth || pos.y > _screenHeight);
                }
                // COLLISIONS
                float r0, r, xd, yd;
                SpaceObject so = null;
                bool _checkFurther = true;
                Vector3 pos1;

                if (ObjectsPresented(CalculatingType.Player) && !_playerController.IsInvincible)
                {
                    r0 = _playerController.Radius;
                    pos0 = _playerTransform.position;

                    if (ObjectsPresented(CalculatingType.UfoBullet))
                    {
                        if (IsColliding(_ufoBulletsList, ref so))
                        {
                            _checkFurther = false;
                            DestroyPlayer();
                            DestroyUfoBullet(so);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, ref so))
                        {
                            DestroyPlayer();
                            DestroyAsteroid(so);
                        }
                    }
                }
                if (ObjectsPresented(CalculatingType.UFO))
                {
                    r0 = _ufo.Radius;
                    pos0 = _ufoTransform.position;

                    if (ObjectsPresented(CalculatingType.PlayerBullet))
                    {
                        if (IsColliding(_playerBulletsList, ref so))
                        {
                            _checkFurther = false;
                            DestroyUFO();
                            DestroyPlayerBullet(so);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, ref so))
                        {
                            DestroyUFO();
                            //DestroyAsteroid(so); - в тз не указано
                        }
                    }
                }
                _needToClear = false;
                if (ObjectsPresented(CalculatingType.PlayerBullet) && ObjectsPresented(CalculatingType.Asteroids))
                {
                    foreach (var a in _playerBulletsList)
                    {
                        r0 = a.Key.Radius;
                        pos0 = a.Value.position;
                        if (IsColliding(_asteroidsList, ref so))
                        {
                            _playerBulletsClearList.Add(a.Key);
                            _asteroidsClearList.Add(so);
                            _needToClear = true;
                            continue;
                        }
                    }
                }
                if (_needToClear)
                {
                    foreach (var p in _playerBulletsClearList)
                    {
                        DestroyPlayerBullet(p);
                    }
                    foreach (var a in _asteroidsClearList)
                    {
                        DestroyAsteroid(a);
                    }
                    _playerBulletsList.Clear();
                    _asteroidsClearList.Clear();
                }
                //


               

                bool IsColliding(in Dictionary<SpaceObject, Transform> list, ref SpaceObject _hitObject)
                {
                    foreach (var so in list)
                    {
                        pos1 = so.Value.position;
                        xd = pos0.x - pos1.x; xd *= xd;
                        yd = pos0.y - pos1.y; yd *= yd;
                        r = r0 - so.Key.Radius; r *= r;
                        if (xd + yd < r)
                        {
                            _hitObject = so.Key;
                            return true;
                        }
                    }
                    return false;
                }

                void DestroyPlayer()
                {
                    _playerController.MakeDestroyed();
                    _presentedObjectsMask[(int)CalculatingType.Player] = false;
                }
                void DestroyUFO()
                {
                    _ufo.MakeDestroyed();
                    _presentedObjectsMask[(int)CalculatingType.UFO] = false;
                }
                void DestroyUfoBullet(in SpaceObject so)
                {
                    so.MakeDestroyed();
                    _ufoBulletsList.Remove(so);
                    _presentedObjectsMask[(int)CalculatingType.UfoBullet] = _ufoBulletsList.Count != 0;
                }
                void DestroyPlayerBullet(in SpaceObject so)
                {
                    so.MakeDestroyed();
                    _playerBulletsList.Remove(so);
                    _presentedObjectsMask[(int)CalculatingType.PlayerBullet] = _playerBulletsList.Count != 0;
                }
                void DestroyAsteroid(in SpaceObject so)
                {
                    so.MakeDestroyed();
                    _asteroidsList.Remove(so);
                    _presentedObjectsMask[(int)CalculatingType.Asteroids] = _asteroidsList.Count != 0;
                }

                bool ObjectsPresented(CalculatingType co)
                {
                    return _presentedObjectsMask[(int)co];
                }
            }
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
        }
                
    }
}
