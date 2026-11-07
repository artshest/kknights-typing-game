using System;
using KK.App.Flow.Screens;
using KK.App.Texts;

namespace KK.App.Tests.Fakes
{
    public class FakeMenuScreen : IMenuScreen
    {
        public event Action LibrarySelected;
        public event Action<string> ArticleSelected;

        public bool IsShown { get; private set; }
        public string ShownLink { get; private set; }
        public int LinkErrorCount { get; private set; }
        public bool HasSubscribers => LibrarySelected != null || ArticleSelected != null;

        public void Show(string link)
        {
            IsShown = true;
            ShownLink = link;
            LinkErrorCount = 0;
        }

        public void ShowLinkError()
        {
            LinkErrorCount++;
        }

        public void Hide()
        {
            IsShown = false;
        }

        public void ClickLibrary()
        {
            LibrarySelected?.Invoke();
        }

        public void ClickArticle(string link)
        {
            ArticleSelected?.Invoke(link);
        }
    }

    // Правила ссылок сайта здесь не нужны: состояние только спрашивает валидатор и слушается ответа
    public class FakeArticleLinkValidator : IArticleLinkValidator
    {
        public const string ValidLink = "https://kknights.com/posts/valid";

        public bool IsValid(string link)
        {
            return link == ValidLink;
        }
    }
}
