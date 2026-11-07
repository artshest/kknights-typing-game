using System;
using KK.App.Flow.Screens;
using KK.App.Texts;

namespace KK.App.Tests.Fakes
{
    public class FakeLoadingScreen : ILoadingScreen
    {
        public enum Status { Hidden, Loading, Error, UnexpectedError }

        public event Action RetryRequested;
        public event Action MenuRequested;

        public Status Current { get; private set; } = Status.Hidden;
        public TextLoadErrorReason? ErrorReason { get; private set; }
        public bool HasRetrySubscribers => RetryRequested != null;
        public bool HasMenuSubscribers => MenuRequested != null;

        public void ShowLoading()
        {
            Current = Status.Loading;
            ErrorReason = null;
        }

        public void ShowError(TextLoadErrorReason reason)
        {
            Current = Status.Error;
            ErrorReason = reason;
        }

        public void ShowUnexpectedError()
        {
            Current = Status.UnexpectedError;
            ErrorReason = null;
        }

        public void Hide()
        {
            Current = Status.Hidden;
        }

        public void ClickRetry()
        {
            RetryRequested?.Invoke();
        }

        public void ClickMenu()
        {
            MenuRequested?.Invoke();
        }
    }
}
