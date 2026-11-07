using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Typing
{
    public class KeyboardView : MonoBehaviour
    {
        [Serializable]
        private class Key
        {
            public string Id;
            public Image Background;
            public TMP_Text Label;
            public Graphic HomeMark;

            [NonSerialized] public Sprite IdleSprite;
            [NonSerialized] public Color IdleLabelColor;
        }

        [SerializeField]
        private Key[] _keys = Array.Empty<Key>();
        [SerializeField]
        private Sprite _activeSprite;
        [SerializeField]
        private Sprite _mistakeSprite;
        [SerializeField]
        private Color _activeLabelColor = new Color32(0x1A, 0x12, 0x08, 0xFF);

        private Key _highlightedKey;
        private Key _highlightedShift;

        private void Awake()
        {
            foreach (Key key in _keys)
            {
                key.IdleSprite = key.Background.sprite;
                key.IdleLabelColor = key.Label != null ? key.Label.color : default;
            }
        }

        public void Highlight(char ch, bool hasMistake)
        {
            Clear();

            if (!KeyboardLayout.TryFind(ch, out string keyId, out string shiftKeyId))
                return;

            Sprite sprite = hasMistake ? _mistakeSprite : _activeSprite;
            _highlightedKey = SetHighlighted(Find(keyId), sprite);
            if (shiftKeyId != null)
                _highlightedShift = SetHighlighted(Find(shiftKeyId), sprite);
        }

        public void Clear()
        {
            Restore(_highlightedKey);
            Restore(_highlightedShift);
            _highlightedKey = null;
            _highlightedShift = null;
        }

        private Key Find(string id)
        {
            foreach (Key key in _keys)
            {
                if (key.Id == id)
                    return key;
            }

            return null;
        }

        private Key SetHighlighted(Key key, Sprite sprite)
        {
            if (key == null)
                return null;

            key.Background.sprite = sprite;
            SetLabelColor(key, _activeLabelColor);
            return key;
        }

        private static void Restore(Key key)
        {
            if (key == null)
                return;

            key.Background.sprite = key.IdleSprite;
            SetLabelColor(key, key.IdleLabelColor);
        }

        private static void SetLabelColor(Key key, Color color)
        {
            if (key.Label != null)
                key.Label.color = color;
            if (key.HomeMark != null)
                key.HomeMark.color = color;
        }
    }
}
