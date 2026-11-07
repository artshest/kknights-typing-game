using KK.App.Texts;
using System;

namespace KK.App.Flow.Screens
{
    public interface ILoadingScreen
    {
        event Action RetryRequested;
        // Доступен и во время загрузки: уход отменяет запрос, второй загрузки не будет
        event Action MenuRequested;

        void ShowLoading();
        void ShowError(TextLoadErrorReason reason);
        void ShowUnexpectedError();
        void Hide();
    }
}
