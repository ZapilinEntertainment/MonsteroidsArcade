using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class ObjectsManager
    {
        private Dictionary<SpaceObjectType, PoolCell> _pools;
        private Explosion[] _explosions;
        private Transform _objectsHost;
        private int _lastUsedExplosionIndex = 0;
        private readonly SpaceObjectType _bulletsMask = SpaceObjectType.PlayerBullet | SpaceObjectType.UFOBullet;
        private const string PREFABS_PATH = "Prefabs/";
        private const int MAX_EXPLOSIONS_COUNT = 4;
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            else return true;
        }
        public static bool operator ==(ObjectsManager A, ObjectsManager B)
        {
            if (ReferenceEquals(A, null))
            {
                return ReferenceEquals(B, null);
            }
            return A.Equals(B);
        }
        public static bool operator !=(ObjectsManager A, ObjectsManager B)
        {
            return !(A == B);
        }
        public ObjectsManager(Transform i_objectsHost)
        {
            PoolCell bulletCell = new PoolCell(PREFABS_PATH + "bullet");
            _pools = new Dictionary<SpaceObjectType, PoolCell>()
            {
                { SpaceObjectType.SmallAsteroid, new PoolCell(PREFABS_PATH + "smallAsteroid")},
                { SpaceObjectType.MediumAsteroid, new PoolCell(PREFABS_PATH + "mediumAsteroid")},
                { SpaceObjectType.BigAsteroid, new PoolCell(PREFABS_PATH + "bigAsteroid")},
                { SpaceObjectType.PlayerBullet, bulletCell},
                { SpaceObjectType.UFOBullet, bulletCell}
            };
            _objectsHost = i_objectsHost;
            //
            _explosions = new Explosion[MAX_EXPLOSIONS_COUNT];
            GameObject _explosionPref = Resources.Load<GameObject>(PREFABS_PATH + "explosion");
            Explosion e;
            for (int i = 0; i < MAX_EXPLOSIONS_COUNT; i++)
            {
                e = Object.Instantiate(_explosionPref, _objectsHost).GetComponent<Explosion>();
                e.Deactivate();
                _explosions[i] = e;
            }
        }

        public SpaceObject CreateObject(SpaceObjectType type, Vector3 position)
        {
            if (_pools.ContainsKey(type))
            {
                if ((type & _bulletsMask) == 0)  return _pools[type].Instantiate(_objectsHost, position);
                else
                {
                    var b = _pools[type].Instantiate(_objectsHost, position);
                    (b as Bullet).ChangeOwner(type == SpaceObjectType.PlayerBullet);
                    return b;
                }
            }
            else
            {
                if (type == SpaceObjectType.UFO)
                {
                    UFO ufo = Object.Instantiate(
                        Resources.Load<GameObject>(PREFABS_PATH + "ufo"), position, Quaternion.identity,_objectsHost
                        ).GetComponent<UFO>();
                    return ufo;
                }
                else
                {
                    Debug.Log("warning - wrong pool master call");
                    return null;
                }
            }
        }
        public void CreateExplosion(Vector3 position, float size)
        {
            _explosions[_lastUsedExplosionIndex++].Explode(position, size);
            if (_lastUsedExplosionIndex >= MAX_EXPLOSIONS_COUNT) _lastUsedExplosionIndex = 0;
        }
        public void ReturnToPool(SpaceObject so)
        {
            if (_pools.ContainsKey(so.ObjectType))
            {
                _pools[so.ObjectType].ReturnToPool(so);
            }
            else Debug.Log("warning - wrong type of pooling object");
        }
        public void ReturnToPool(in ICollection<SpaceObject> list, SpaceObjectType type)
        {
            if (_pools.ContainsKey(type))
            {
                _pools[type].ReturnToPool(list);
            }
            else Debug.Log("warning - wrong type of pooling object");            
        }

        public float GetObjectRawRadius(SpaceObjectType type)
        {
            if (_pools.ContainsKey(type)) return _pools[type].RawRadius;
            else return 1f;
        }

        private class PoolCell
        {
            private Stack<SpaceObject> _pool;
            private GameObject _prefabLink;
            public float RawRadius => _prefabLink.GetComponent<SpaceObject>().RawRadius;
            private bool _isEmpty { get { return _pool.Count == 0; } }          


            public PoolCell(string path)
            {
                _prefabLink = Resources.Load<GameObject>(path);
                _pool = new Stack<SpaceObject>();
            }

            public SpaceObject Instantiate(Transform host, Vector3 position)
            {
                if (_isEmpty) return Object.Instantiate(_prefabLink,  position, Quaternion.identity, host).GetComponent<SpaceObject>();
                else
                {
                    var g = _pool.Pop();
                    g.transform.position = position;
                    g.gameObject.SetActive(true);
                    return g;
                }
            }      
            
            public void ReturnToPool(SpaceObject so)
            {                
                so.Stop();
                so.gameObject.SetActive(false);
                _pool.Push(so);
            }
            public void ReturnToPool(in ICollection<SpaceObject> list)
            {
                int count0 = _pool.Count;
                var arr = new SpaceObject[count0 + list.Count];
                if (count0 != 0)
                {
                    _pool.CopyTo(arr, 0);
                }
                list.CopyTo(arr, count0);
                _pool = new Stack<SpaceObject>(arr);
            }
        }
    }
}
