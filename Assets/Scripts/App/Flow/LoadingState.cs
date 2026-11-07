using Cysharp.Threading.Tasks;
using KK.App.Flow.Screens;
using KK.App.Logging;
using KK.App.Texts;
using KK.Core.Texts;
using System;
using System.Threading;

namespace KK.App.Flow
{
    public class LoadingState : IPayloadState<TextSource>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ILoadingScreen _screen;
        private readonly ITypingTextProviderFactory _providerFactory;
        private readonly TextNormalizationOptions _normalizationOptions;
        private readonly IAppLogger _logger;

        private TextSource _source;
        private CancellationTokenSource _loadCancellation;

        public LoadingState(GameStateMachine stateMachine, ILoadingScreen screen,
            ITypingTextProviderFactory providerFactory, TextNormalizationOptions normalizationOptions, IAppLogger logger)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _normalizationOptions = normalizationOptions ?? throw new ArgumentNullException(nameof(normalizationOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Enter(TextSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _screen.RetryRequested += HandleRetryRequested;
            _screen.MenuRequested += HandleMenuRequested;
            _loadCancellation = new CancellationTokenSource();
            LoadAsync(source, _loadCancellation.Token).Forget();
        }

        public void Exit()
        {
            // Ушли с экрана загрузки — запрос больше никому не нужен.
            // ?. — на случай, если Enter бросил на проверке аргумента и не создал источник отмены
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
            _loadCancellation = null;

            _screen.RetryRequested -= HandleRetryRequested;
            _screen.MenuRequested -= HandleMenuRequested;
            _screen.Hide();
        }

        private async UniTaskVoid LoadAsync(TextSource source, CancellationToken ct)
        {
            _screen.ShowLoading();
            string text;

            try
            {
                string rawText = await _providerFactory.Create(source).GetTextAsync(ct);
                text = TextNormalizer.Normalize(rawText, _normalizationOptions);

                if (text.Length == 0)
                    throw new InvalidOperationException("После нормализации текст пустой: в нём нет символов, которые можно набрать");
            }
            // Глушим только свою отмену (вышли из состояния). Любая другая отмена — ошибка, и игрок должен её увидеть
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (TextLoadException e)
            {
                // Предупреждение, а не исключение: нет сети или статьи — ожидаемая ситуация, а не баг в коде
                _logger.Warning($"Текст не загружен ({e.Reason}): {e}");
                _screen.ShowError(e.Reason);
                return;
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                _screen.ShowUnexpectedError();
                return;
            }

            // Провайдер может не уважать токен и вернуть текст уже после выхода из состояния
            if (ct.IsCancellationRequested)
                return;

            // Переход вне try: ошибка следующего состояния — не ошибка загрузки, и показывать её на уже
            // закрытом экране загрузки нельзя
            _stateMachine.Enter<TypingState, LoadedText>(new LoadedText(text, source));
        }

        private void HandleRetryRequested()
        {
            // Повторный вход: Exit отменит прошлую загрузку, Enter начнёт новую из того же источника
            _stateMachine.Enter<LoadingState, TextSource>(_source);
        }

        private void HandleMenuRequested()
        {
            // Exit отменит загрузку, если она ещё идёт
            _stateMachine.Enter<MenuState>();
        }
    }
}
