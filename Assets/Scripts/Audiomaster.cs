using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsteroidsArcade
{
    public enum AudioEffectType : byte { PlayerShot, UfoShot, Blast, ButtonClicked, GameOver, UfoDefeated, ShipDestroyed}
    public sealed class Audiomaster : MonoBehaviour
    {
        [SerializeField] private float _musicVolume = 1f, _effectVolume = 1f;
        private Dictionary<AudioEffectType, AudioClip> _clipList;
        private AudioSource[] _asources;
        private AudioSource _musicAsource,_defeatedSource;
        private bool _prepared = false;
        private int _lastUsedSourceIndex = 0;
        private const int MAX_EFFECT_SOURCES = 5;
        private const string SOUNDPATH = "Soundclips/";
        public static Audiomaster Current { get; private set; }
        
        public void Prepare()
        {
            if (!_prepared)
            {
                Current = this;
                transform.position = Vector3.zero;
                _asources = new AudioSource[MAX_EFFECT_SOURCES];
                AudioSource aso;
                for (int i = 0; i < MAX_EFFECT_SOURCES; i++)
                {
                    aso = gameObject.AddComponent<AudioSource>();
                    aso.loop = false;
                    aso.volume = _effectVolume;
                    _asources[i] = aso;
                }
                //
                _clipList = new Dictionary<AudioEffectType, AudioClip>() {
                    { AudioEffectType.Blast, Resources.Load<AudioClip>(SOUNDPATH + "blast")},
                     { AudioEffectType.PlayerShot, Resources.Load<AudioClip>(SOUNDPATH + "playershot")},
                      { AudioEffectType.UfoShot, Resources.Load<AudioClip>(SOUNDPATH + "ufoshot")},
                       { AudioEffectType.ButtonClicked, Resources.Load<AudioClip>(SOUNDPATH + "button")},
                        { AudioEffectType.GameOver, Resources.Load<AudioClip>(SOUNDPATH + "gameover")},
                         { AudioEffectType.UfoDefeated, Resources.Load<AudioClip>(SOUNDPATH + "ufodefeated")},
                          { AudioEffectType.ShipDestroyed, Resources.Load<AudioClip>(SOUNDPATH + "shipdestroyed")}
                };
                //
                _musicAsource = gameObject.AddComponent<AudioSource>();
                _musicAsource.clip = Resources.Load<AudioClip>(SOUNDPATH + "maintheme");
                _musicAsource.volume = _musicVolume;
                _musicAsource.loop = true;
                _musicAsource.Play();
                //
                _prepared = true;
            }
        }

        public void Inner_PlayEffect(AudioEffectType type)
        {
            if (_clipList.ContainsKey(type))
            {
                AudioSource aso = _asources[_lastUsedSourceIndex++];
                if (aso.isPlaying) aso.Stop();
                aso.clip = _clipList[type];                
                aso.Play();
                if (_lastUsedSourceIndex >= MAX_EFFECT_SOURCES) _lastUsedSourceIndex = 0;
                if (type == AudioEffectType.GameOver) _defeatedSource = aso;
            }
            else Debug.Log(type.ToString() + " clip type not found");
        }

        public void NewSessionStarted()
        {
            _defeatedSource?.Stop();
        }

        public static void PlayEffect(AudioEffectType type) { Current.Inner_PlayEffect(type); }
    }
}
