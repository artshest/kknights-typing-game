namespace KK.App.Texts
{
    // Формат ссылок знает только инфраструктура (она же ходит на сайт), а проверить ссылку до загрузки
    // нужно меню — поэтому порт
    public interface IArticleLinkValidator
    {
        bool IsValid(string link);
    }
}
