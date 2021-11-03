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
        private bool _prepared = false, _isPaused = false, _needToClear = false, 
            _waitForNewAsteroids = false, _needToCreateNewAsteroids = false;
        private SpaceObjectType _asteroidsMask;
        private float _screenWidth, _screenHeight, _canvasScale;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private UFO _ufo;
        private Transform _playerTransform, _ufoTransform;
        private Dictionary<SpaceObject, Transform> _asteroidsList, _playerBulletsList, _ufoBulletsList; 
       
        private HashSet<SpaceObject> _playerBulletsClearList, _asteroidsClearList; // список для очищения. Hashset так как не допускает повторений и порядок неважен 
        private List<(Vector3 pos, Vector3 dir, bool mediumSize)> _asteroidsCreateList;
        
        private ObjectsManager _poolManager;
        private GameSettings _gameSettings;
        private System.Action<float> _canvasScaleUpdateEvent;
        private const float SHIP_EXPLOSION_SIZE = 6f, BULLET_EXPLOSION_SIZE = 3f;
        public void Prepare(GameManager gm, PlayerController pc, RectTransform _gameZone)
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
            _asteroidsCreateList = new List<(Vector3, Vector3, bool)>();
            //           
            _poolManager = new ObjectsManager(_gameZone);
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

        #region objects creating
        public void CreateBigAsteroids(int count)
        {
            float x, widthPart = _screenWidth / (_screenWidth + _screenHeight), 
                r = _poolManager.GetObjectRawRadius(SpaceObjectType.BigAsteroid) * _canvasScale;
            Vector3 position, direction, center = new Vector3(_screenWidth / 2f, _screenHeight / 2f, 0);
            SpaceObject so;
            for (int i = 0; i < count; i++)
            {
                x = Random.value;
                if (x > widthPart)
                {
                    //spawns at left & right sides
                    x = (x - widthPart) / (1f - widthPart);
                    if (x > 0.5f)
                    {
                        x = (x - 0.5f) / 0.5f;
                        position = new Vector3(_screenWidth + r, _screenHeight * x, 0f);
                        direction = Quaternion.Euler(0f, 0f, Random.value * 25f - 90f) * (center  -position).normalized;
                    }
                    else
                    {
                        x /= 0.5f;
                        position = new Vector3(-r, _screenHeight * x, 0f);
                        direction = Quaternion.Euler(0f, 0f, Random.value * 25f - 90f) * (center - position).normalized;
                    }
                }
                else
                {
                    //spawns at up & down sides
                    x /= widthPart;
                    if (x > 0.5f)
                    {
                        x = (x - 0.5f) / 0.5f;
                        position = new Vector3(_screenWidth * x, _screenHeight + r, 0f);
                        direction = Quaternion.Euler(0f, 0f, Random.value * 25f - 90f) * (center - position).normalized;
                    }
                    else
                    {
                        x /= 0.5f;
                        position = new Vector3(_screenWidth * x, -r, 0f);
                        direction = Quaternion.Euler(0f, 0f, Random.value * 25f - 90f) * (center - position).normalized;
                    }
                }
                CreatePoolingObject_StandartSpeed(
                    SpaceObjectType.BigAsteroid,
                    position,
                    direction,
                    _asteroidsList
                    );
            }
            _presentedObjectsMask[(int)CalculatingType.Asteroids] = true;
            _waitForNewAsteroids = false;
        }
        public void CreateBullet(Vector3 pos, Vector3 dir, bool playersBullet)
        {           
            CreatePoolingObject_StandartSpeed(
                playersBullet ? SpaceObjectType.PlayerBullet : SpaceObjectType.UFOBullet,
                pos,
                dir,
                playersBullet ? _playerBulletsList : _ufoBulletsList
                );         
        }
        private void CreatePoolingObject_StandartSpeed(SpaceObjectType type, Vector3 position, Vector3 direction, in Dictionary<SpaceObject, Transform> hostlist)
        {
            CreatePoolingObject_CustomSpeed(type, position, direction * _gameSettings.GetObjectSpeed(type), hostlist);
        }
        private void CreatePoolingObject_CustomSpeed(SpaceObjectType type, Vector3 position, Vector3 moveVector, in Dictionary<SpaceObject, Transform> hostlist)
        {
            var so = _poolManager.CreateObject(type, position);
            so.SetMoveVector(moveVector);
            hostlist.Add(so, so.transform);
            _canvasScaleUpdateEvent += so.ChangeCanvasScale;
            if (_canvasScale != 1f) so.ChangeCanvasScale(_canvasScale);
            _presentedObjectsMask[(int)type.DefineCalculatingType()] = hostlist.Count != 0;
        }
        
        public UFO LaunchUFO()
        {
            const float BORDER = 0.2f;
            float height = (BORDER + Random.value * (1f - 2f * BORDER)) * _screenHeight;
            Vector3 position, direction;
            if (Random.value > 0.5f)
            {
                position = new Vector3(0f, height, 0f);
                direction = Vector3.right;
            }
            else
            {
                position = new Vector3(_screenWidth, height, 0f);
                direction = Vector3.left;
            }
            
            if (_ufo == null)
            {
                _ufo = _poolManager.CreateObject(SpaceObjectType.UFO, position) as UFO;
                _ufo.AssignLinks(_gameSettings, this, _playerTransform);
                _ufoTransform = _ufo.transform;                
                _canvasScaleUpdateEvent += _ufo.ChangeCanvasScale;
                _ufo.ChangeCanvasScale(_canvasScale);
            }
            else
            {
                _ufoTransform.position = position;
                _ufo.gameObject.SetActive(true);
            }
            _ufo.SetMoveVector(direction * _gameSettings.GetObjectSpeed(SpaceObjectType.UFO));

            _presentedObjectsMask[(int)CalculatingType.UFO] = true;
            return _ufo;
        }
        #endregion

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
                    _playerTransform.position = CheckPosition(_playerTransform.position + _playerController.MoveVector * _simulationTick, _playerController.Radius);
                }
                
                if (ObjectsPresented(CalculatingType.UFO))
                {
                    _ufoTransform.position = CheckPosition(_ufoTransform.position + _ufo.MoveVector * _simulationTick, _ufo.Radius);
                }
                //
                if (ObjectsPresented(CalculatingType.PlayerBullet))
                {
                    foreach (var a in _playerBulletsList)
                    {
                        t = a.Value;
                        pos0 = t.position + a.Key.MoveVector * _simulationTick;
                        t.position = CheckPosition(pos0, a.Key.Radius);
                    }
                }
                //
                if (ObjectsPresented(CalculatingType.UfoBullet))
                {
                    foreach (var a in _ufoBulletsList)
                    {
                        t = a.Value;
                        pos0 = t.position + a.Key.MoveVector * _simulationTick;
                        t.position = CheckPosition(pos0, a.Key.Radius);
                    }
                }
                //
                if (ObjectsPresented(CalculatingType.Asteroids))
                {
                    foreach (var a in _asteroidsList)
                    {
                        t = a.Value;
                        t.position = CheckPosition(t.position + a.Key.MoveVector * _simulationTick,a.Key.Radius);
                    }
                }               

                Vector3 CheckPosition(Vector3 pos, in float radius)
                {
                    float r2 = radius;
                    if (pos.x < -r2) pos.x = _screenWidth + pos.x + r2 * 2f;
                    else if (pos.x > _screenWidth + r2) pos.x -= (_screenWidth + 2f * r2);
                    if (pos.y < -r2) pos.y = _screenHeight + pos.y + r2 * 2f;
                    else if (pos.y > _screenHeight + r2) pos.y -= (_screenHeight + 2f * r2);
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
                            Audiomaster.PlayEffect(AudioEffectType.Blast);
                            _poolManager.CreateExplosion(pos0, SHIP_EXPLOSION_SIZE);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, out so))
                        {
                            DestroyPlayer();
                            DestroyAsteroid(so);
                            Audiomaster.PlayEffect(AudioEffectType.Blast);
                            _poolManager.CreateExplosion(pos0, SHIP_EXPLOSION_SIZE);
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
                            Audiomaster.PlayEffect(AudioEffectType.Blast);
                            _poolManager.CreateExplosion(pos0, SHIP_EXPLOSION_SIZE);
                            Audiomaster.PlayEffect(AudioEffectType.UfoDefeated);
                        }
                    }
                    if (_checkFurther && ObjectsPresented(CalculatingType.Asteroids))
                    {
                        if (IsColliding(_asteroidsList, out so))
                        {
                            DestroyUFO();
                            DestroyAsteroid(so);
                            Audiomaster.PlayEffect(AudioEffectType.Blast);
                            _poolManager.CreateExplosion(pos0, SHIP_EXPLOSION_SIZE);
                        }
                    }
                }
                _needToClear = false;
                if (ObjectsPresented(CalculatingType.Asteroids))
                {
                    if (ObjectsPresented(CalculatingType.PlayerBullet))
                    {
                        int count = _asteroidsList.Count, score = 0;
                        foreach (var a in _playerBulletsList)
                        {
                            r0 = a.Key.Radius;
                            pos0 = a.Value.position;
                            SpaceObject hitSO;
                            if (IsColliding(_asteroidsList, out hitSO))
                            {
                                _playerBulletsClearList.Add(a.Key);
                                _poolManager.CreateExplosion(pos0, BULLET_EXPLOSION_SIZE);
                                if (!_asteroidsClearList.Contains(hitSO))
                                {
                                    _asteroidsClearList.Add(hitSO);
                                    _needToClear = true;
                                    if (hitSO.ObjectType != SpaceObjectType.SmallAsteroid)
                                    {
                                        bool bigAsteroid = hitSO.ObjectType == SpaceObjectType.BigAsteroid;
                                        Vector3 dir = hitSO.MoveVector.normalized;
                                        _asteroidsCreateList.Add((hitSO.transform.position, dir, bigAsteroid));
                                        _needToCreateNewAsteroids = true;
                                        score += bigAsteroid ? _gameSettings.BigAsteroidScore : _gameSettings.MediumAsteroidScore;
                                    }
                                    else score += _gameSettings.SmallAsteroidScore;
                                }
                                Audiomaster.PlayEffect(AudioEffectType.Blast);
                            }
                        }
                        if (score != 0) _gameManager.AddScore(score);
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
                if (_needToCreateNewAsteroids)
                {
                    float speed;
                    SpaceObjectType type;
                    float angle = _gameSettings.AsteroidsSpawnAngle;
                    Quaternion rot0 = Quaternion.Euler(0f, 0f,angle),rot1 = Quaternion.Euler(0f, 0f, -angle);
                    Vector3 v;
                    foreach (var a in _asteroidsCreateList)
                    {
                        type = a.mediumSize ? SpaceObjectType.MediumAsteroid : SpaceObjectType.SmallAsteroid;
                        speed = _gameSettings.GetObjectSpeed(type);
                        v = rot0 * a.dir;
                        CreatePoolingObject_CustomSpeed(type, a.pos + v, v * speed, _asteroidsList);
                        v = rot1 * a.dir;
                        CreatePoolingObject_CustomSpeed(type, a.pos + v, v * speed, _asteroidsList);
                    }
                    _asteroidsCreateList.Clear();
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
                    _ufo.gameObject.SetActive(false);
                    _presentedObjectsMask[(int)CalculatingType.UFO] = false;
                    _gameManager.UfoDestroyed();
                }                
                void DestroyAsteroid(in SpaceObject so)
                {
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
            _ufoBulletsList.Remove(so);
            _presentedObjectsMask[(int)CalculatingType.UfoBullet] = _ufoBulletsList.Count != 0;
            _poolManager.ReturnToPool(so);
        }
        public void DestroyPlayerBullet(in SpaceObject so)
        {
            _playerBulletsList.Remove(so);
            _presentedObjectsMask[(int)CalculatingType.PlayerBullet] = _playerBulletsList.Count != 0;
            _poolManager.ReturnToPool(so);
        }

        public void DestroyAllObjects()
        {
            _playerController.Hide();
            if (_presentedObjectsMask[(int)CalculatingType.UFO])
            {
                _ufo.Stop();
                _ufo.gameObject.SetActive(false);
            }
            if (_presentedObjectsMask[(int)CalculatingType.PlayerBullet])
            {
                _poolManager.ReturnToPool(_playerBulletsList.Keys, SpaceObjectType.PlayerBullet);
            }
            _playerBulletsList.Clear();
            //
            if (_presentedObjectsMask[(int)CalculatingType.UfoBullet])
            {
                _poolManager.ReturnToPool(_ufoBulletsList.Keys, SpaceObjectType.UFOBullet);
            }
            _ufoBulletsList.Clear();
            //
            if (_presentedObjectsMask[(int)CalculatingType.Asteroids])
            {
                foreach (var a in _asteroidsList.Keys)
                {
                    _poolManager.ReturnToPool(a);
                }
            }
            _asteroidsList.Clear();
            //
            _presentedObjectsMask.SetAll(false);
        }
        public void SetPause(bool x)
        {
            _isPaused = x;
        }
    }
}
