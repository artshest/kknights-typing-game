using KK.App.Texts;

namespace KK.Presentation.Loading
{
    // Тексты только из символов белого списка TextNormalizer: статический атлас шрифта собран ровно из них,
    // поэтому «», — и … нарисовались бы квадратами. Проверяет LoadErrorFormatterTests
    public static class LoadErrorFormatter
    {
        public const string Loading = "Загрузка текста...";
        public const string Unexpected = "Что-то пошло не так. Попробуй ещё раз.";

        public static string Format(TextLoadErrorReason reason)
        {
            return reason switch
            {
                TextLoadErrorReason.Network => "Не удалось связаться с сайтом. Проверь подключение к интернету.",
                TextLoadErrorReason.NotFound => "Статья не найдена. Проверь ссылку.",
                TextLoadErrorReason.Timeout => "Сайт слишком долго не отвечает. Попробуй ещё раз.",
                TextLoadErrorReason.BadFormat => "Не получилось прочитать статью: сайт ответил в незнакомом формате.",
                // Сообщение об ошибке не должно само падать — иначе игрок вместо текста ошибки получит пустой экран
                _ => Unexpected
            };
        }
    }
}
