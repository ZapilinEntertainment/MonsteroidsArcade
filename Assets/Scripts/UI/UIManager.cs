using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public enum GameUIStatus : byte { MainMenu, PauseWindow, Playmode, GameOver}
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private MainMenuUI _mainMenuScript;
        [SerializeField] private GameUI _gameUiScript;
        [SerializeField] private FailPanelUI _failPanelScript;
        [SerializeField] private RectTransform _gameZone;

        private GameUIStatus _status;
        private GameManager _gameManager;
      
        public RectTransform GameZone => _gameZone;

        public void Prepare(GameManager gm, PlayerController pc)
        {
            _gameManager = gm;
            _mainMenuScript.Prepare(gm);
            _gameUiScript.Prepare(gm, pc);
            _failPanelScript.Prepare(gm);
        }
        public void ChangeStatus(GameUIStatus i_status)
        {
            _status = i_status;
            switch (_status)
            {
                case GameUIStatus.MainMenu:
                    {
                        _mainMenuScript.ActivateWindow(false);
                        _gameUiScript.gameObject.SetActive(false);
                        _failPanelScript.gameObject.SetActive(false);
                        break;
                    }
                case GameUIStatus.PauseWindow:
                    {
                        _mainMenuScript.ActivateWindow(true);
                        _gameUiScript.gameObject.SetActive(true);
                        _failPanelScript.gameObject.SetActive(false);
                        break;
                    }
                case GameUIStatus.Playmode:
                    {
                        _mainMenuScript.gameObject.SetActive(false);
                        _gameUiScript.gameObject.SetActive(true);
                        _failPanelScript.gameObject.SetActive(false);
                        break;
                    }
                case GameUIStatus.GameOver:
                    {
                        _mainMenuScript.gameObject.SetActive(false);
                        _gameUiScript.gameObject.SetActive(true);
                        _failPanelScript.gameObject.SetActive(true);
                        break;
                    }
            }
        }
    }
}
