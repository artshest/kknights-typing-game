using System;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Decor
{
    [RequireComponent(typeof(Image))]
    public class SpriteLoop : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] _frames = Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)]
        private float _framesPerSecond = 6f;

        private Image _image;
        private int _currentFrame = -1;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (_frames.Length == 0)
                return;

            int frame = (int)(Time.time * _framesPerSecond) % _frames.Length;
            // Смена спрайта помечает канвас на перестроение — только когда кадр действительно сменился
            if (frame == _currentFrame)
                return;

            _currentFrame = frame;
            _image.sprite = _frames[frame];
        }
    }
}
