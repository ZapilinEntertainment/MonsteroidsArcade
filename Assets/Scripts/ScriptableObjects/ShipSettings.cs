﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    // объекты настроек хранятся по адресу Settings/Game
    [CreateAssetMenu(fileName = "ShipSettings", menuName = "ScriptableObjects/ShipSettingsObject", order = 1)]
    public sealed class ShipSettings : ScriptableObject
    {
        [SerializeField] private float _maxSpeed = 5f, _rotationSpeed = 35f, _acceleration = 1f, _keyboardRotationSpeed = 90f; 

        public float MaxSpeed => _maxSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float Acceleration => _acceleration;
        public float KeyboardControlsSensivity => _keyboardRotationSpeed;
    }
}