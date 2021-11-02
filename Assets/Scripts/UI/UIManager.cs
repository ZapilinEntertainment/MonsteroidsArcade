using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public enum GameUIStatus : byte { MainMenu, PauseWindow, Playmode, GameOver}
    public sealed class UIManager : MonoBehaviour, IPausable
    {
        [SerializeField] private MainMenuUI _mainMenuScript;
        [SerializeField] private GameUI _gameUiScript;
        [SerializeField] private FailPanelUI _failPanelScript;
        [SerializeField] private RectTransform _gameZone;
        [SerializeField] private RawImage _background;
        [SerializeField] private float _backgroundSpeed = 0.1f;

        private GameUIStatus _status;
        private GameManager _gameManager;
        private bool _isPaused = false;
      
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

        public void SetPause(bool x)
        {
            _isPaused = x;
        }

        private void Update()
        {
            if (!_isPaused)
            {
                var r = _background.uvRect;
                float speed = _backgroundSpeed * Time.deltaTime;
                r.position += Vector2.right * speed;
                //float h, s, v;
                //Color.RGBToHSV(_background.color, out h, out s, out v);
               // _background.color = Color.HSVToRGB(Mathf.Clamp01(h + speed * 0.1f), s, v);
                _background.uvRect = r;
            }
        }
    }
}
