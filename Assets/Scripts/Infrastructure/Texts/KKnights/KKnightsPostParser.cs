using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace KK.Infrastructure.Texts.KKnights
{
    /// <summary>
    /// Парсит ответ API kknights.com (/api/v1/post/{slug}/) в текст поста.
    /// </summary>
    public static class KKnightsPostParser
    {
        public class ParsedPost
        {
            public string Title;
            public string Slug;
            public string Text;
            public List<string> Paragraphs = new List<string>();
        }

        // Теги, закрывающие абзац: всё между ними становится отдельным абзацем
        private static readonly Regex ParagraphBreakRegex = new Regex(
            @"</\s*(?:p|li|h[1-6]|blockquote|div)\s*>|<\s*br\s*/?\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TagRegex = new Regex(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Парсит JSON поста. На сайте два формата редактора:
        /// "blocks" — массив content с HTML в json.content каждого блока;
        /// "document" — дерево узлов в document.doc (формат ProseMirror/TipTap).
        /// </summary>
        public static ParsedPost Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON пустой", nameof(json));

            JObject root = JObject.Parse(json);

            var post = new ParsedPost
            {
                Title = (string)root["title"],
                Slug = (string)root["slug"]
            };

            if (root["content"] is JArray blocks)
                ParseBlocks(blocks, post.Paragraphs);

            if (root["document"]?["doc"] is JObject doc)
                ParseDocument(doc, post.Paragraphs);

            if (post.Paragraphs.Count == 0)
                throw new FormatException($"В посте '{post.Slug}' не найдено текста");

            post.Text = string.Join("\n", post.Paragraphs);
            return post;
        }

        /// <summary>
        /// Формат "blocks": текст лежит HTML-строкой в json.content;
        /// блоки без текста (embed) пропускаются.
        /// </summary>
        private static void ParseBlocks(JArray blocks, List<string> paragraphs)
        {
            foreach (JToken block in blocks)
            {
                if (block["json"]?["content"] is not JValue { Type: JTokenType.String } content)
                    continue;

                paragraphs.AddRange(ExtractParagraphs((string)content));
            }
        }

        /// <summary>
        /// Формат "document": рекурсивно обходит дерево узлов. Узлы "text" дают текст,
        /// "hardBreak" и конец любого блочного узла (paragraph, heading, sideGallery…)
        /// завершают абзац. Узлы без текста (gallery, divider) ничего не добавляют.
        /// </summary>
        private static void ParseDocument(JObject doc, List<string> paragraphs)
        {
            WalkNode(doc, new StringBuilder(), paragraphs);
        }

        private static void WalkNode(JToken node, StringBuilder buffer, List<string> paragraphs)
        {
            switch ((string)node["type"])
            {
                case "text":
                    buffer.Append((string)node["text"]);
                    return;
                case "hardBreak":
                    FlushParagraph(buffer, paragraphs);
                    return;
            }

            if (node["content"] is JArray children)
            {
                foreach (JToken child in children)
                    WalkNode(child, buffer, paragraphs);
            }

            FlushParagraph(buffer, paragraphs);
        }

        private static void FlushParagraph(StringBuilder buffer, List<string> paragraphs)
        {
            string text = NormalizeText(buffer.ToString());
            buffer.Clear();

            if (text.Length > 0)
                paragraphs.Add(text);
        }

        /// <summary>
        /// Делит HTML-фрагмент на абзацы и превращает каждый в чистый текст.
        /// </summary>
        private static IEnumerable<string> ExtractParagraphs(string html)
        {
            foreach (string part in ParagraphBreakRegex.Split(html))
            {
                string text = HtmlToText(part);
                if (text.Length > 0)
                    yield return text;
            }
        }

        private static string HtmlToText(string html)
        {
            string text = TagRegex.Replace(html, string.Empty);
            return NormalizeText(WebUtility.HtmlDecode(text));
        }

        private static string NormalizeText(string text)
        {
            // Неразрывный пробел набрать нельзя — заменяем обычным
            text = text.Replace((char)0xA0, ' ');
            text = WhitespaceRegex.Replace(text, " ");
            return text.Trim();
        }
    }
}
