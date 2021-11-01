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
        private float _smallAsteroidMinSpeed = 5f;
        [SerializeField]
        private float _smallAsteroidMaxSpeed = 10f,
            _mediumAsteroidMinSpeed = 3f, _mediumAsteroidMaxSpeed = 6f,
            _bigAsteroidMinSpeed = 1.5f, _bigAsteroidMaxSpeed = 3f;
        [Space]
        [SerializeField] private int _playerLivesCount = 3;
        [SerializeField] private int _startAsteroidsCount = 2, _asteroidsPerLevelSurplus = 2;
        [SerializeField] private float _startInvincibilityTime = 3f, _respawnDelay = 0.5f, _playerFireCooldown = 1f / 3f,
            _bulletSpeed = 10f, _nextRoundDelay = 2f;
        [Space]
        [Header("UFO Settings")]
        [SerializeField] private float _ufoMinTime = 20f;
        [SerializeField] private float _ufoMaxTime = 40f, _ufoFlightTime = 10f, _ufoMinFireCooldown = 2f, _ufoMaxFireColldown = 5f;
         [Header("Коэффициент скоростей движения всех объектов")][SerializeField] private float _speedCoefficient = 10f;
        public int PlayerLivesCount => _playerLivesCount;
        public int StartAsteroidsCount => _startAsteroidsCount;
        public int AsteroidsPerLevelSurplus => _asteroidsPerLevelSurplus;
        public float StartInvincibilityTime => _startInvincibilityTime;
        public float RespawnDelay => _respawnDelay;
        public float PlayerFireCooldown => _playerFireCooldown;
        public float NextRoundDelay => _nextRoundDelay;

        public float GetObjectSpeed(SpaceObjectType type)
        {
            switch (type)
            {
                case SpaceObjectType.BigAsteroid: 
                    return (Random.value * (_bigAsteroidMaxSpeed - _bigAsteroidMinSpeed) + _bigAsteroidMinSpeed) * _speedCoefficient;
                case SpaceObjectType.MediumAsteroid: return (Random.value * (_mediumAsteroidMaxSpeed - _mediumAsteroidMinSpeed) + _mediumAsteroidMinSpeed) * _speedCoefficient;
                case SpaceObjectType.SmallAsteroid: return (Random.value * (_smallAsteroidMaxSpeed - _smallAsteroidMinSpeed) + _smallAsteroidMinSpeed) * _speedCoefficient;
                case SpaceObjectType.UFOBullet:
                case SpaceObjectType.PlayerBullet: return _bulletSpeed * _speedCoefficient;
                case SpaceObjectType.UFO: return (_speedCoefficient * 0.1f) / _ufoFlightTime;
                default:  return _speedCoefficient;
            }
        }

        public float GetUfoTime()
        {
            return Random.value * (_ufoMaxTime - _ufoMinTime) + _ufoMinTime;
        }

        public float GetUfoFireCooldown()
        {
            return Random.value * (_ufoMaxFireColldown - _ufoMinFireCooldown) + _ufoMinFireCooldown;
        }
    }
}
