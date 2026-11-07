using System;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Typing
{
    public class SegmentedProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image[] _segments = Array.Empty<Image>();
        [SerializeField]
        private Color _filledColor = new Color32(0xF0, 0x88, 0x3E, 0xFF);
        [SerializeField]
        private Color _emptyColor = new Color32(0x2A, 0x28, 0x36, 0xFF);

        // progress — от 0 до 1
        public void SetProgress(float progress)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(progress) * _segments.Length);
            for (int i = 0; i < _segments.Length; i++)
                _segments[i].color = i < filled ? _filledColor : _emptyColor;
        }
    }
}
