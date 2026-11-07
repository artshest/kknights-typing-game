using System;

namespace KK.App.Texts
{
    public enum TextSourceKind 
    { 
        Library, 
        Article 
    }

    public sealed class TextSource : IEquatable<TextSource>
    {
        public TextSourceKind Kind { get; }
        public string ArticleLink { get; }

        private TextSource(TextSourceKind kind, string articleLink = null)
        {
            Kind = kind;
            ArticleLink = articleLink;
        }

        public static TextSource Library() => new TextSource(TextSourceKind.Library);
        
        public static TextSource Article(string articleLink)
        {
            if (articleLink == null)
            {
                throw new ArgumentNullException(nameof(articleLink));
            }

            if (string.IsNullOrWhiteSpace(articleLink))
            {
                throw new ArgumentException("Ссылка на статью пустая", nameof(articleLink));
            }

            return new TextSource(TextSourceKind.Article, articleLink);
        }

        public bool Equals(TextSource other)
        {
            return other is not null
                && Kind == other.Kind
                && ArticleLink == other.ArticleLink;
        }

        public override bool Equals(object obj) => Equals(obj as TextSource);
        public override int GetHashCode() => HashCode.Combine(Kind, ArticleLink);
        public static bool operator ==(TextSource left, TextSource right) => Equals(left, right);
        public static bool operator !=(TextSource left, TextSource right) => !Equals(left, right);
    }
}
