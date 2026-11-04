using KK.App.Texts;
using System;

namespace KK.App.Flow.Screens
{
    public interface ILoadingScreen
    {
        event Action RetryRequested;

        void ShowLoading();
        void ShowError(TextLoadErrorReason reason);
        void ShowUnexpectedError();
        void Hide();
    }
}
