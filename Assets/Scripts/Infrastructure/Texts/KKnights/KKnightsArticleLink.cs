using System;

namespace KK.Infrastructure.Texts.KKnights
{
    public static class KKnightsArticleLink
    {
        private const string Host = "kknights.com";
        private const string ApiUrlFormat = "https://kknights.com/api/v1/post/{0}/";

        // Публичная ссылка на статью — /posts/{slug}, адрес API — /api/v1/post/{slug}/; принимаем оба
        private static readonly string[] ArticlePath = { "posts" };
        private static readonly string[] ApiPath = { "api", "v1", "post" };

        public static bool TryParseSlug(string link, out string slug)
        {
            slug = null;
            if (string.IsNullOrWhiteSpace(link))
                return false;

            link = link.Trim();
            // Из адресной строки браузера ссылку часто копируют без схемы
            if (!link.Contains("://"))
                link = "https://" + link;

            if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
                return false;
            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                return false;
            if (!string.Equals(uri.Host, Host, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uri.Host, "www." + Host, StringComparison.OrdinalIgnoreCase))
                return false;

            // AbsolutePath уже без ?query и #fragment
            string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string candidate = MatchSlug(segments, ArticlePath) ?? MatchSlug(segments, ApiPath);
            if (candidate == null || !IsValidSlug(candidate))
                return false;

            slug = candidate;
            return true;
        }

        public static string ToApiUrl(string slug)
        {
            return string.Format(ApiUrlFormat, slug);
        }

        private static string MatchSlug(string[] segments, string[] prefix)
        {
            if (segments.Length != prefix.Length + 1)
                return null;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (!string.Equals(segments[i], prefix[i], StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            return segments[prefix.Length];
        }

        // Slug подставляется в адрес API, поэтому пускаем только то, что встречается в slug сайта:
        // иначе "%2F" или ".." изменили бы путь запроса. Кириллица в пути приходит как %D0%..
        // и тоже отсекается — на сайте slug транслитерированы
        private static bool IsValidSlug(string slug)
        {
            foreach (char ch in slug)
            {
                bool allowed = (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9')
                    || ch == '-' || ch == '_';
                if (!allowed)
                    return false;
            }

            return true;
        }
    }
}
