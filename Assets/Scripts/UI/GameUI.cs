using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonsteroidsArcade
{
    public sealed class GameUI : MonoBehaviour,IPausable
    {
        [SerializeField] private TMP_Text _scoreLabel, _livesLabel;
        private GameManager _gameManager;
        private PlayerController _playerController;
        private bool _prepared = false, _isPaused = false;

        public void Prepare(GameManager i_gm, PlayerController i_pc)
        {
            _gameManager = i_gm;
            _playerController = i_pc;
            _prepared = true;
        }

        public void SetPause(bool x)
        {
            _isPaused = x;
            _scoreLabel.text = "Score: " + _gameManager.Score.ToString();
            _livesLabel.text = _playerController.LivesCount.ToString();
        }

        private void Update()
        {
            if (!_isPaused && _prepared )
            {
                _scoreLabel.text = "Score: " + _gameManager.Score.ToString();
                _livesLabel.text = _playerController.LivesCount.ToString();
            }
        }
    }
}
