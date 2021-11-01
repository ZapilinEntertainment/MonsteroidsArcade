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
        [SerializeField] protected SpaceObjectType _type;
        [SerializeField] private float _radius = 1f;
        protected float _canvasScale = 1f;
        public SpaceObjectType ObjectType => _type;

        virtual public Vector3 MoveVector => _moveVector * _canvasScale;
        protected Vector3 _moveVector;
        public float Radius => _radius * _canvasScale;


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
        virtual public void Stop()
        {
            _moveVector = Vector3.zero;
        }
        virtual public void SetMoveVector(Vector3 v)
        {
            _moveVector = v;
        }

        virtual public void ChangeCanvasScale(float x)
        {
            _canvasScale = x;
        }
    }
}
