namespace KK.App.Typing
{
    // Ввод создаётся на каждый раунд: так он слушает клавиатуру только во время набора,
    // а не пока игрок, например, вводит ссылку в меню
    public interface ITypingInputFactory
    {
        // Освобождает созданный ввод тот, кто его попросил
        ITypingInput Create();
    }
}
