using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public sealed class Bullet : SpaceObject
    {
        [SerializeField] private Image _sprite;
        private bool _playersBullet = true;
        private float _distanceTravelled;
        private Vector3 _lastPos;
        private static float MAX_DISTANCE_SQR = 512f;
        private static MotionCalculator _motionCalculator;

        public static void Prepare(float maxDistance, MotionCalculator i_mc)
        {
            MAX_DISTANCE_SQR = maxDistance * maxDistance;
            _motionCalculator = i_mc;
        }

        public override void Stop()
        {
            base.Stop();
            _distanceTravelled = 0f;
        }

        private void LateUpdate()
        {
            Vector3 p = transform.position;
            _distanceTravelled += (p - _lastPos).sqrMagnitude;
            _lastPos = p;
            if (_distanceTravelled >= MAX_DISTANCE_SQR)
            {
                if (_playersBullet) _motionCalculator.DestroyPlayerBullet(this);
                else _motionCalculator.DestroyUfoBullet(this);
            }
        }

        public override void SetMoveVector(Vector3 v)
        {
            base.SetMoveVector(v);
            _lastPos = transform.position;
        }

        public void ChangeOwner(bool playerBullet)
        {
            _playersBullet = playerBullet;
            if (_playersBullet)
            {
                _sprite.color = GameConstants.PlayerBulletColor;
                _type = SpaceObjectType.PlayerBullet;
            }
            else
            {
                _sprite.color = GameConstants.PlayerBulletColor;
                _type = SpaceObjectType.UFOBullet;
            }
        }
    }
}
