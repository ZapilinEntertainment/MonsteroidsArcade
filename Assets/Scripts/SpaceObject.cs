using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MonsteroidsArcade
{
    [Flags]
    public enum SpaceObjectType : byte { Player = 1, SmallAsteroid = 2, MediumAsteroid = 4, BigAsteroid = 8, 
        UFO = 16, PlayerBullet = 32, UFOBullet = 64}
    public static class SpaceObjectTypeExtension
    {
        public static CalculatingType DefineCalculatingType(this SpaceObjectType type)
        {
            switch (type)
            {
                case SpaceObjectType.Player: return CalculatingType.Player;
                case SpaceObjectType.UFO: return CalculatingType.UFO;
                case SpaceObjectType.PlayerBullet: return CalculatingType.PlayerBullet;
                case SpaceObjectType.UFOBullet: return CalculatingType.UfoBullet;
                default: return CalculatingType.Asteroids;
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

        private void Start()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }

        virtual public void MakeDestroyed()
        {

        }
    }
}
