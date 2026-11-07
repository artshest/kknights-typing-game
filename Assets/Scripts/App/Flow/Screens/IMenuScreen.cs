using System;

namespace KK.App.Flow.Screens
{
    public interface IMenuScreen
    {
        event Action LibrarySelected;
        // Строка из поля ввода как есть: проверяет её состояние, а не экран
        event Action<string> ArticleSelected;

        void Show(string link);
        void ShowLinkError();
        void Hide();
    }
}
