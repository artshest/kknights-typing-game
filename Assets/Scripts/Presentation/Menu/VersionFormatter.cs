namespace KK.Presentation.Menu
{
    public static class VersionFormatter
    {
        // Хэш — метаданные SemVer через «+»: нужен разработчику, чтобы по скриншоту найти коммит,
        // игроку в релизе — шум. Файл с хэшем может кончаться переводом строки — отсюда Trim.
        public static string Format(string version, string commit, bool showCommit)
        {
            var text = "v" + version;
            var trimmedCommit = commit?.Trim();
            return showCommit && !string.IsNullOrEmpty(trimmedCommit)
                ? text + "+" + trimmedCommit
                : text;
        }
    }
}
