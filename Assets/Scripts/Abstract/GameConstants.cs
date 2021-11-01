using UnityEngine;
namespace MonsteroidsArcade
{
    public static class GameConstants
    {
        // #settings системные настройки
        private const string CONTROLS_CONFIG_KEY = "controlByKeyboardOnly", HIGHSCORE_KEY = "highscore";
        public static readonly Color PlayerBulletColor = Color.green, UfoBulletColor = Color.red;

        public static bool IsControlKeyboardOnly()
        {
            return PlayerPrefs.GetInt(CONTROLS_CONFIG_KEY, 0) == 0;
        }
        public static void SetControlOption(bool keyboardOnly)
        {
            PlayerPrefs.SetInt(CONTROLS_CONFIG_KEY, keyboardOnly ? 0 : 1);
        }

        public static int GetHighscore()
        {
            return PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
        }
        public static void SetHighscore(int x)
        {
            if (GetHighscore() < x) PlayerPrefs.SetInt(HIGHSCORE_KEY, x);
        }
    }
}
