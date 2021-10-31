using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade {
    public enum CalculatingType : byte { Player = 0, UFO, Asteroids, PlayerBullet, UfoBullet, Total }
    public sealed class MotionCalculator : MonoBehaviour,IPausable
    {       
        // Рассчитывает передвижение и коллизию всех объектов.
        [SerializeField] private float _simulationTick = 1f / 120f;
        private BitArray _presentedObjectsMask = new BitArray((int)CalculatingType.Total, false);
        private bool _prepared = false, _isPaused = false, _needToClear = false, _waitForNewAsteroids = false;
        private SpaceObjectType _asteroidsMask;
        private float _screenWidth, _screenHeight, _canvasScale;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private UFO _ufo;
        private Transform _playerTransform, _ufoTransform;

        private Dictionary<SpaceObject, Transform> _asteroidsList, _playerBulletsList, _ufoBulletsList; 
       
        private HashSet<SpaceObject> _playerBulletsClearList, _asteroidsClearList; // список для очищения. Hashset так как не допускает повторений и порядок неважен 
        private PoolManager _poolManager;
        private GameSettings _gameSettings;
        private System.Action<float> _canvasScaleUpdateEvent;
        public void Prepare(GameManager gm, PlayerController pc)
        {
            _gameManager = gm;
            _isPaused = _gameManager.IsPaused;
            _gameManager.SubscribeToPauseEvent(this);
            _gameSettings = _gameManager.GameSettings;

            _playerController = pc;
            _playerTransform = pc.transform;
            _canvasScaleUpdateEvent += pc.ChangeCanvasScale;           
            Time.fixedDeltaTime = _simulationTick;            
            //
            _asteroidsMask = SpaceObjectType.SmallAsteroid | SpaceObjectType.MediumAsteroid | SpaceObjectType.BigAsteroid;
            //
            _asteroidsList = new Dictionary<SpaceObject, Transform>();
            _playerBulletsList = new Dictionary<SpaceObject, Transform>();
            _ufoBulletsList = new Dictionary<SpaceObject, Transform>();

            _playerBulletsClearList = new HashSet<SpaceObject>();
            _asteroidsClearList = new HashSet<SpaceObject>();
            //           
            _poolManager = new PoolManager(_gameManager.GameZoneHost);
            //
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            Bullet.Prepare(_screenWidth, _gameManager.MotionCalculator);
            //
            _prepared = true;
        }

        private void Start()
        {
            _canvasScale = FindObjectOfType<Canvas>().scaleFactor;
            if (_canvasScale != 1f) _canvasScaleUpdateEvent(_canvasScale);
        }

        public void CreateBigAsteroids(int count)
        {
            float pc = 1f / count, x,
                widthPart = _screenWidth / (_screenWidth + _screenHeight) * 0.5f,
                heightPart = 0.5f - widthPart,
                perimeter = 2f * _screenWidth + 2f * _screenHeight
                ;
            // делим периметр экрана на 4 неравноценные части
            Vector3 position;
            SpaceObject so;
            for (int i = 0; i < count; i++)
            {
                x = Random.value;
                if (x > 0.5f)
                {
                    if (x < widthPart)
                    { // нижняя граница экрана
                        position = new Vector3(((i + x) * pc) * perimeter, 0f, 0f);
                    }
                    else
                    {// правая граница экрана
                        position = new Vector3(_screenWidth, ((i + x) * pc - widthPart) * perimeter, 0f);
                    }
                }
                else
                {
                    x -= 0.5f;
                    if (x < widthPart)
                    { // верхняя граница экрана
                        position = new Vector3((1f - heightPart - (i + x) * pc) * perimeter, _screenHeight, 0f);
                    }
                    else
                    { // левая граница экрана
                        position = new Vector3(0f, (1f - (i + x) * pc) * perimeter, 0f);
                    }
                }

                position = new Vector3(Random.value * _screenWidth, Random.value * _screenHeight, 0f);
                so = _poolManager.CreateObject(SpaceObjectType.BigAsteroid, position);
                so.SetMoveVector((Quaternion.Euler(0f, 0f, Random.value * 360f) * Vector3.up) * _gameSettings.GetObjectSpeed(SpaceObjectType.BigAsteroid));                
                _asteroidsList.Add(so, so.transform);
                _canvasScaleUpdateEvent += so.ChangeCanvasScale;
                if (_canvasScale != 1f) so.ChangeCanvasScale(_canvasScale);
            }
            _presentedObjectsMask[(int)CalculatingType.Asteroids] = true;
            _waitForNewAsteroids = false;
        }
        public void CreateBullet(Vector3 pos, Vector3 dir, bool playersBullet)
        {
            var so = _poolManager.CreateObject(playersBullet ? SpaceObjectType.PlayerBullet : SpaceObjectType.UFOBullet, pos);
            so.SetMoveVector(dir);
            _canvasScaleUpdateEvent += so.ChangeCanvasScale;
            if (_canvasScale != 1f) so.ChangeCanvasScale(_canvasScale);
            if (playersBullet)
            {
                _playerBulletsList.Add(so, so.transform);
                _presentedObjectsMask[(int)CalculatingType.PlayerBullet] = true;
            }
            else
            {
                _ufoBulletsList.Add(so, so.transform);
                _presentedObjectsMask[(int)CalculatingType.UfoBullet] = true;
            }
        }

        private void FixedUpdate()
        {            
            if (_prepared & !_isPaused)
            {
                Vector3 pos0;
                Transform t;
                // MOVEMENT
                _presentedObjectsMask[(int)CalculatingType.Player] = !_playerController.IsDestroyed;
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
                        t.position = CheckPosition(pos0);
                    }
                }
                //
                if (ObjectsPresented(CalculatingType.UfoBullet))
                {
                    foreach (var a in _ufoBulletsList)
                    {
                        t = a.Value;
                        pos0 = t.position + a.Key.MoveVector * _simulationTick;
                        t.position = CheckPosition(pos0);
                    }
                }
                //
                if (ObjectsPresented(CalculatingType.Asteroids))
                {
                    foreach (var a in _asteroidsList)
                    {
                        t = a.Value;
                        t.position = CheckPosition(t.position + a.Key.MoveVector * _simulationTick);
                    }
                }

                

                Vector3 CheckPosition(Vector3 pos)
                {
                    if (pos.x < 0f) pos.x = _screenWidth + pos.x;
                    else if (pos.x > _screenWidth) pos.x -= _screenWidth;
                    if (pos.y < 0f) pos.y = _screenHeight + pos.y;
                    else if (pos.y > _screenHeight) pos.y -= _screenHeight;
                    return pos;
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
                        if (IsColliding(_ufoBulletsList, out so))
                        {
                            _checkFurther = false;
                            DestroyPlayer();
                            DestroyUfoBullet(so);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, out so))
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
                        if (IsColliding(_playerBulletsList, out so))
                        {
                            _checkFurther = false;
                            DestroyUFO();
                            DestroyPlayerBullet(so);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, out so))
                        {
                            DestroyUFO();
                            //DestroyAsteroid(so); - в тз не указано
                        }
                    }
                }
                _needToClear = false;
                if (ObjectsPresented(CalculatingType.Asteroids))
                {
                    if (ObjectsPresented(CalculatingType.PlayerBullet))
                    {
                        foreach (var a in _playerBulletsList)
                        {
                            r0 = a.Key.Radius;
                            pos0 = a.Value.position;
                            SpaceObject hitSO;
                            if (IsColliding(_asteroidsList, out hitSO))
                            {
                                _playerBulletsClearList.Add(a.Key);
                                _asteroidsClearList.Add(hitSO);
                                _needToClear = true;
                            }
                        }
                    }
                }
                else
                {
                    if (!_waitForNewAsteroids)
                    {
                        _waitForNewAsteroids = true;
                        _gameManager.NextRound();
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
                    _playerBulletsClearList.Clear();
                    _asteroidsClearList.Clear();
                }
                //


               

                bool IsColliding(in Dictionary<SpaceObject, Transform> list, out SpaceObject _hitObject)
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
                    _hitObject = null;
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
                void DestroyAsteroid(in SpaceObject so)
                {
                    so.MakeDestroyed();
                    _asteroidsList.Remove(so);
                    _presentedObjectsMask[(int)CalculatingType.Asteroids] = _asteroidsList.Count != 0;
                    _poolManager.ReturnToPool(so);
                }

                bool ObjectsPresented(CalculatingType co)
                {
                    return _presentedObjectsMask[(int)co];
                }
            }
        }
        private void Update()
        {
            float sh = Screen.height, sw = Screen.width;
            if (sh != _screenHeight || sw != _screenWidth)
            {
                _screenHeight = sh;
                _screenWidth = sw;
                StartCoroutine(WaitForCanvasScaleChange());
            }
        }
        IEnumerator WaitForCanvasScaleChange()
        {
            yield return 0.25f;
            _canvasScale = FindObjectOfType<Canvas>().scaleFactor;
            _canvasScaleUpdateEvent(_canvasScale);
        }
        public void DestroyUfoBullet(in SpaceObject so)
        {
            so.MakeDestroyed();
            _ufoBulletsList.Remove(so);
            _presentedObjectsMask[(int)CalculatingType.UfoBullet] = _ufoBulletsList.Count != 0;
            _poolManager.ReturnToPool(so);
        }
        public void DestroyPlayerBullet(in SpaceObject so)
        {
            so.MakeDestroyed();
            _playerBulletsList.Remove(so);
            _presentedObjectsMask[(int)CalculatingType.PlayerBullet] = _playerBulletsList.Count != 0;
            _poolManager.ReturnToPool(so);
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
        }
    }
}
