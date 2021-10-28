using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public enum SpaceObjectType : byte { PlayerShip = 0, SmallAsteroid, MediumAsteroid, BigAsteroid, UFO, PlayerBullet, UFOBullet, Total }
    public static class SpaceObjectTypeExtension
    {
        public static bool IsFastObject(this SpaceObjectType type)
        {
            switch (type)
            {                
                case SpaceObjectType.PlayerShip:
                case SpaceObjectType.SmallAsteroid:
                case SpaceObjectType.PlayerBullet:
                case SpaceObjectType.UFOBullet:
                    return true;
                default:
                    return false;
            }
        } 
    }
    
    public class SpaceObject : MonoBehaviour
    {
        [SerializeField] private SpaceObjectType _type;
        [SerializeField] private float _radius = 1f;
        public SpaceObjectType ObjectType => _type;
        
        public Vector3 MoveVector { get; protected set; }
        public float Radius => _radius;
        public bool Simulating { get; protected set; }

        private void Start()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
