using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KK.Presentation.Typing
{
    // Сплошной фон под текущим символом. <mark> в TMP рисуется поверх букв, поэтому тёмную букву
    // на оранжевом фоне им не сделать — фон отдельной Image, стоящей в иерархии перед текстом
    [RequireComponent(typeof(Image))]
    public class TypingCaret : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private Color _normalColor = new Color32(0xF0, 0x88, 0x3E, 0xFF);
        [SerializeField]
        private Color _mistakeColor = new Color32(0xE0, 0x52, 0x4A, 0xFF);

        private Image _image;
        private int _index = -1;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.enabled = false;
            // Pivot в левом нижнем углу: тогда localPosition — это сразу угол прямоугольника символа
            ((RectTransform)transform).pivot = Vector2.zero;
            PlaceBehindText();
        }

        // UI рисуется в порядке иерархии: окажись фон после текста, он закроет букву
        private void PlaceBehindText()
        {
            Transform text = _text.transform;
            if (transform.parent != text.parent)
            {
                Debug.LogError($"{nameof(TypingCaret)} должен лежать рядом с текстом, иначе он не окажется за буквами", this);
                return;
            }

            if (transform.GetSiblingIndex() > text.GetSiblingIndex())
                transform.SetSiblingIndex(text.GetSiblingIndex());
        }

        public void Show(int index, bool hasMistake)
        {
            _index = index;
            _image.color = hasMistake ? _mistakeColor : _normalColor;
            Refresh();
        }

        public void Hide()
        {
            _index = -1;
            _image.enabled = false;
        }

        // Текст переносится заново при изменении размера окна, поэтому позиция пересчитывается каждый кадр
        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_index < 0)
                return;

            if (_text.havePropertiesChanged)
                _text.ForceMeshUpdate();

            // Индекс символа в textInfo совпадает с индексом в исходной строке: теги форматтера
            // в characterInfo не попадают, а экранированный «<» остаётся одним символом
            TMP_TextInfo info = _text.textInfo;
            if (_index >= info.characterCount)
            {
                _image.enabled = false;
                return;
            }

            TMP_CharacterInfo ch = info.characterInfo[_index];
            TMP_LineInfo line = info.lineInfo[ch.lineNumber];

            var (left, right) = ch.isVisible
                ? CaretLayout.Glyph(ch.origin, ch.xAdvance, ch.bottomLeft.x, ch.topRight.x)
                : CaretLayout.Space(ch.origin, ch.xAdvance,
                    NeighbourGlyphEdge(info, _index - 1, ch.lineNumber, rightEdge: true),
                    NeighbourGlyphEdge(info, _index + 1, ch.lineNumber, rightEdge: false));
            Vector2 min = ToSnappedParentPoint(new Vector3(left, line.descender));
            Vector2 max = ToSnappedParentPoint(new Vector3(right, line.ascender));

            var rect = (RectTransform)transform;
            rect.localPosition = min;
            rect.sizeDelta = max - min;
            _image.enabled = true;
        }

        private static float? NeighbourGlyphEdge(TMP_TextInfo info, int index, int lineNumber, bool rightEdge)
        {
            if (index < 0 || index >= info.characterCount)
                return null;

            TMP_CharacterInfo neighbour = info.characterInfo[index];
            if (!neighbour.isVisible || neighbour.lineNumber != lineNumber)
                return null;

            return rightEdge ? neighbour.topRight.x : neighbour.bottomLeft.x;
        }

        // Канвас масштабируется под экран, и дробные края плашки округляются у разных букв по-разному.
        // Привязка углов к целым пикселям экрана делает края одинаково чёткими
        private Vector2 ToSnappedParentPoint(Vector3 textLocalPoint)
        {
            Canvas canvas = _image.canvas;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, _text.transform.TransformPoint(textLocalPoint));
            screen = new Vector2(Mathf.Round(screen.x), Mathf.Round(screen.y));

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, screen, camera, out Vector2 local);
            return local;
        }
    }
}
