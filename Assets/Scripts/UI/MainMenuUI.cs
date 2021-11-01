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
        [SerializeField] private TMP_Text _controlsButtonLabel;
        private GameManager _gameManager;

        public void Prepare(GameManager gm)
        {
            _gameManager = gm;
        }
        public void ActivateWindow(bool activateContinueButton)
        {
            RedrawControlsButton();
            _continueButtonGO.SetActive(activateContinueButton);
            gameObject.SetActive(true);
        }
        public void ContinueButton()
        {
            _gameManager.PauseSwitch();
            gameObject.SetActive(false);
        }
        public void StartGameButton()
        {
            _gameManager.StartNewSession();
            gameObject.SetActive(false);
        }
        public void ControlsButton()
        {
            GameConstants.SetControlOption(!GameConstants.IsControlKeyboardOnly());
            RedrawControlsButton();
        }
        public void ExitButton()
        {
            Application.Quit();
        }

        private void RedrawControlsButton()
        {
            _controlsButtonLabel.text = GameConstants.IsControlKeyboardOnly() ? "Keyboard-only input" : "Keyboard + mouse input";
        }
    }
}
