namespace KK.App.Flow
{
    // Общий предок нужен машине: выйти можно из любого состояния, а вход у них разный — с данными и без
    public interface IExitableState
    {
        void Exit();
    }

    public interface IState : IExitableState
    {
        void Enter();
    }

    public interface IPayloadState<in TPayload> : IExitableState
    {
        void Enter(TPayload payload);
    }
}
