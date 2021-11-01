using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public sealed class FailPanelUI : MonoBehaviour
    {
        private GameManager _gameManager;
        public void RetryButton()
        {
            _gameManager.StartNewSession();
            gameObject.SetActive(false);
        }
        public void MenuButton()
        {
            _gameManager.ReturnToMenu();
            gameObject.SetActive(false);
        }

        public void Prepare(GameManager i_gm)
        {
            _gameManager = i_gm;
        }
    }
}
