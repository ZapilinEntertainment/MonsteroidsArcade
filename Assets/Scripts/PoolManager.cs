using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class PoolManager
    {
        private Dictionary<SpaceObjectType, PoolCell> _pools;
        private Transform _objectsHost;
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            else return true;
        }
        public static bool operator ==(PoolManager A, PoolManager B)
        {
            if (ReferenceEquals(A, null))
            {
                return ReferenceEquals(B, null);
            }
            return A.Equals(B);
        }
        public static bool operator !=(PoolManager A, PoolManager B)
        {
            return !(A == B);
        }
        public PoolManager(Transform i_objectsHost)
        {
            _pools = new Dictionary<SpaceObjectType, PoolCell>()
            {
                { SpaceObjectType.SmallAsteroid, new PoolCell("smallAsteroid")},
                { SpaceObjectType.MediumAsteroid, new PoolCell("mediumAsteroid")},
                { SpaceObjectType.BigAsteroid, new PoolCell("bigAsteroid")},
                { SpaceObjectType.PlayerBullet, new PoolCell("playerBullet")},
                { SpaceObjectType.UFOBullet, new PoolCell("ufoBullet")}
            };
            _objectsHost = i_objectsHost;
        }

        public SpaceObject CreateObject(SpaceObjectType type, Vector3 position)
        {
            if (_pools.ContainsKey(type))
            {
                return _pools[type].Instantiate(_objectsHost, position);
            }
            else
            {
                Debug.Log("warning - wrong pool master call");
                return null;
            }
        }

        public void ReturnToPool(SpaceObject so)
        {
            if (_pools.ContainsKey(so.ObjectType))
            {
                _pools[so.ObjectType].ReturnToPool(so);
            }
            else Debug.Log("warning - wrong type of pooling object");
        }

        private class PoolCell
        {
            private Stack<SpaceObject> _pool;
            private GameObject _prefabLink;
            private bool _isEmpty { get { return _pool.Count == 0; } }
            private const string RESOURCES_PATH = "Prefabs/";


            public PoolCell(string name)
            {
                _prefabLink = Resources.Load<GameObject>(RESOURCES_PATH + name);
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
                _pool.Push(so);
                so.Stop();
                so.gameObject.SetActive(false);
            }
        }
    }
}
