using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonsteroidsArcade
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _continueButtonGO;
        [SerializeField] private TMP_Text _controlsButtonLabel, _highscoreLabel;
        private GameManager _gameManager;

        public void Prepare(GameManager gm)
        {
            _gameManager = gm;
        }
        public void ActivateWindow(bool activateContinueButton)
        {
            RedrawControlsButton();
            _continueButtonGO.SetActive(activateContinueButton);
            int hs = GameConstants.GetHighscore();
            if (hs != 0)
            {
                _highscoreLabel.text = "Best result: " + hs.ToString();
                _highscoreLabel.gameObject.SetActive(true);
            }
            else
            {
                _highscoreLabel.gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
        }
        public void ContinueButton()
        {
            _gameManager.PauseSwitch();
            Audiomaster.PlayEffect(AudioEffectType.ButtonClicked);
            gameObject.SetActive(false);
        }
        public void StartGameButton()
        {
            _gameManager.StartNewSession();
            Audiomaster.PlayEffect(AudioEffectType.ButtonClicked);
            gameObject.SetActive(false);
        }
        public void ControlsButton()
        {
            Audiomaster.PlayEffect(AudioEffectType.ButtonClicked);
            GameConstants.SetControlOption(!GameConstants.IsControlKeyboardOnly());
            RedrawControlsButton();
        }
        public void ExitButton()
        {
            Audiomaster.PlayEffect(AudioEffectType.ButtonClicked);
            Application.Quit();
        }

        private void RedrawControlsButton()
        {
            _controlsButtonLabel.text = GameConstants.IsControlKeyboardOnly() ? "Keyboard-only input" : "Keyboard + mouse input";
        }
    }
}
