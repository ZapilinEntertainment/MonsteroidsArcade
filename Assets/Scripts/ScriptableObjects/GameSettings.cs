using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MonsteroidsArcade
{
    // объекты настроек хранятся по адресу Settings/Game
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettingsObject", order = 1)]
    public sealed class GameSettings : ScriptableObject
    {
        [Header("Настройка скоростей астероидов")]
        [SerializeField]
        private float _smallAsteroidMinSpeed = 5f, _smallAsteroidMaxSpeed = 10f,
            _mediumAsteroidMinSpeed = 3f, _mediumAsteroidMaxSpeed = 6f,
            _bigAsteroidMinSpeed = 1.5f, _bigAsteroidMaxSpeed = 3f;

        public float GetObjectSpeed(SpaceObjectType type)
        {
            switch (type)
            {
                case SpaceObjectType.BigAsteroid: return Random.value * (_bigAsteroidMaxSpeed - _bigAsteroidMinSpeed) + _bigAsteroidMinSpeed;
                case SpaceObjectType.MediumAsteroid: return Random.value * (_mediumAsteroidMaxSpeed - _mediumAsteroidMinSpeed) + _mediumAsteroidMinSpeed;
                case SpaceObjectType.SmallAsteroid: return Random.value * (_smallAsteroidMaxSpeed - _smallAsteroidMinSpeed) + _smallAsteroidMinSpeed;
                default:  return 1f;
            }
        }
    }
}
