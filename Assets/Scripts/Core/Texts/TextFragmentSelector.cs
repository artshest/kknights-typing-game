using System;
using System.Collections.Generic;

namespace KK.Core.Texts
{
    // Набирает фрагмент из целых соседних абзацев: стартовый выбирается случайно, затем добавляются соседи,
    // пока фрагмент короче минимума и очередной абзац не выводит за максимум
    public class TextFragmentSelector
    {
        private const string Separator = " ";

        private readonly int _minLength;
        private readonly int _maxLength;
        private readonly Random _random;

        public TextFragmentSelector(TextFragmentOptions options, Random random)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.MinLength <= 0)
                throw new ArgumentException($"Минимальная длина должна быть больше 0, а не {options.MinLength}", nameof(options));
            if (options.MaxLength < options.MinLength)
                throw new ArgumentException(
                    $"Максимальная длина {options.MaxLength} меньше минимальной {options.MinLength}", nameof(options));

            // Копируем значения: объект настроек могут поменять снаружи уже после проверки
            _minLength = options.MinLength;
            _maxLength = options.MaxLength;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public string Select(IReadOnlyList<string> paragraphs)
        {
            if (paragraphs == null) throw new ArgumentNullException(nameof(paragraphs));

            var texts = new List<string>();
            foreach (string paragraph in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph))
                    texts.Add(paragraph);
            }

            if (texts.Count == 0)
                throw new ArgumentException("Нет ни одного непустого абзаца", nameof(paragraphs));

            int start = PickStart(texts);
            int first = start;
            int last = start;
            int length = texts[start].Length;

            // Вперёд предпочтительнее — фрагмент читается в порядке статьи. Назад — только когда
            // статья кончилась или следующий абзац слишком длинный
            while (length < _minLength)
            {
                if (last + 1 < texts.Count && Fits(length, texts[last + 1]))
                {
                    last++;
                    length += Separator.Length + texts[last].Length;
                }
                else if (first - 1 >= 0 && Fits(length, texts[first - 1]))
                {
                    first--;
                    length += Separator.Length + texts[first].Length;
                }
                else
                {
                    break;
                }
            }

            return string.Join(Separator, texts.GetRange(first, last - first + 1));
        }

        private int PickStart(List<string> texts)
        {
            var candidates = new List<int>();
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i].Length <= _maxLength)
                    candidates.Add(i);
            }

            if (candidates.Count > 0)
                return candidates[_random.Next(candidates.Count)];

            // Резать абзацы нельзя, поэтому максимум тут недостижим — берём ближайший к нему
            int shortest = 0;
            for (int i = 1; i < texts.Count; i++)
            {
                if (texts[i].Length < texts[shortest].Length)
                    shortest = i;
            }
            return shortest;
        }

        private bool Fits(int length, string next) => length + Separator.Length + next.Length <= _maxLength;
    }
}
