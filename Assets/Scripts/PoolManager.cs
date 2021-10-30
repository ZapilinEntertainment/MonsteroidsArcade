using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class PoolManager
    {
        private Dictionary<SpaceObjectType, PoolCell> _pools;
        private Transform _objectsHost;

        public PoolManager(Transform _objectsHost)
        {
            _pools = new Dictionary<SpaceObjectType, PoolCell>()
            {
                { SpaceObjectType.SmallAsteroid, new PoolCell("smallAsteroid")},
                { SpaceObjectType.MediumAsteroid, new PoolCell("mediumAsteroid")},
                { SpaceObjectType.BigAsteroid, new PoolCell("bigAsteroid")},
                { SpaceObjectType.PlayerBullet, new PoolCell("playerBullet")},
                { SpaceObjectType.UFOBullet, new PoolCell("ufoBullet")}
            };
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

        private class PoolCell
        {
            private Stack<SpaceObject> _pool;
            private GameObject _prefabLink;
            private bool _isEmpty;
            private const string RESOURCES_PATH = "Prefabs/";


            public PoolCell(string name)
            {
                _prefabLink = Resources.Load<GameObject>(RESOURCES_PATH + name);
                _pool = new Stack<SpaceObject>();
                _isEmpty = true;
            }

            public SpaceObject Instantiate(Transform host, Vector3 position)
            {
                if (_isEmpty) return Object.Instantiate(_prefabLink,  position, Quaternion.identity, host).GetComponent<SpaceObject>();
                else
                {
                    var g = _pool.Pop();
                    g.transform.position = position;
                    g.gameObject.SetActive(true);
                    _isEmpty = _pool.Count == 0;
                    return g;
                }
            }
        }
    }
}
