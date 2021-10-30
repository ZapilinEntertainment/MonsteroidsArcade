using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public sealed class Bullet : SpaceObject
    {
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
            if (_distanceTravelled >= MAX_DISTANCE_SQR) _motionCalculator.DestroyBullet(this);
        }

        public override void SetMoveVector(Vector3 v)
        {
            base.SetMoveVector(v);
            _lastPos = transform.position;
        }
    }
}
