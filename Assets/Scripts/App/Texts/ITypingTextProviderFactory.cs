namespace KK.App.Texts
{
    public interface ITypingTextProviderFactory
    {
        // Может бросить (например, неверная ссылка на статью) — вызывать там, где ошибку покажут игроку
        ITypingTextProvider Create(TextSource source);
    }
}
