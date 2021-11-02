using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsteroidsArcade
{
    public sealed class Explosion : MonoBehaviour, IPausable
    {
        [SerializeField] Image _model;
        [SerializeField] private float _effectTime = 1f;
        private float _progress, _maxScale = 3f;
        private bool _effectInProgress = false, _isPausable = false;

        public void Explode(Vector3 position, float size)
        {
            _progress = 0f;
            Transform t = _model.transform;
            t.position = position;
            t.localScale = Vector3.one;
            _model.color = Color.white;
            _maxScale = size;
            _effectInProgress = true;
            gameObject.SetActive(true);
        }

        public void SetPause(bool x)
        {
            _isPausable = x;
        }

        private void Update()
        {
            if (!_isPausable & _effectInProgress)
            {
                _progress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime / _effectTime);
                if (_progress >= 1f)
                {
                    Deactivate();
                }
                else
                {
                    _model.transform.localScale = Vector3.one * Mathf.Lerp(1f, _maxScale, _progress);
                    _model.color = new Color(1f, 1f, 1f, 1f - _progress);
                }
            }
        }

        public void Deactivate()
        {
            _progress = 0f;
            _effectInProgress = false;
            gameObject.SetActive(false);
        }

    }
}
