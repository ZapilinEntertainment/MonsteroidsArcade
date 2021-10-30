using UnityEngine;
namespace MonsteroidsArcade
{
    public static class GameConstants
    {
        // #settings системные настройки
        private const string CONTROLS_CONFIG_KEY = "controlByKeyboardOnly";

        public static bool IsControlKeyboardOnly()
        {
            return PlayerPrefs.GetInt(CONTROLS_CONFIG_KEY, 0) == 0;
        }
        public static void SetControlOption(bool keyboardOnly)
        {
            PlayerPrefs.SetInt(CONTROLS_CONFIG_KEY, keyboardOnly ? 0 : 1);
        }
    }
}
