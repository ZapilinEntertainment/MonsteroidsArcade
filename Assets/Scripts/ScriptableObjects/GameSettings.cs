using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MonsteroidsArcade
{
    // объекты настроек хранятся по адресу Settings/Game
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettingsObject", order = 1)]
    public sealed class GameSettings : ScriptableObject
    {
        // #settings геймплейные настройки 

        [Header("Настройка скоростей астероидов")]
        [SerializeField]
        private float _smallAsteroidMinSpeed = 5f, _smallAsteroidMaxSpeed = 10f,
            _mediumAsteroidMinSpeed = 3f, _mediumAsteroidMaxSpeed = 6f,
            _bigAsteroidMinSpeed = 1.5f, _bigAsteroidMaxSpeed = 3f;
        [Space]
        [SerializeField] private int _playerLivesCount = 3;
        [SerializeField] private float _startInvincibilityTime = 3f;
        [SerializeField] private float _respawnDelay = 0.5f;

        public int PlayerLivesCount => _playerLivesCount;
        public float StartInvincibilityTime => _startInvincibilityTime;
        public float RespawnDelay => _respawnDelay;

        public float GetObjectSpeed(SpaceObjectType type)
        {
            const float cf = 10f;
            switch (type)
            {
                case SpaceObjectType.BigAsteroid: return (Random.value * (_bigAsteroidMaxSpeed - _bigAsteroidMinSpeed) + _bigAsteroidMinSpeed) * cf;
                case SpaceObjectType.MediumAsteroid: return (Random.value * (_mediumAsteroidMaxSpeed - _mediumAsteroidMinSpeed) + _mediumAsteroidMinSpeed) * cf;
                case SpaceObjectType.SmallAsteroid: return (Random.value * (_smallAsteroidMaxSpeed - _smallAsteroidMinSpeed) + _smallAsteroidMinSpeed) * cf;
                default:  return cf;
            }
        }


    }
}
