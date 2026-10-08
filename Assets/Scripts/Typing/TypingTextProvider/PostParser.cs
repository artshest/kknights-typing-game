using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public class PostParser
{
    public class ParsedPost
    {
        public string Title;
        public string Slug;
        public string Text;
        public List<string> Paragraphs = new List<string>();
    }

    private readonly JArray _root;

    private PostParser(JArray root)
    {
        _root = root;
    }

    /// <summary>
    /// Парсит HTML-страницу и возвращает распарсенный пост.
    /// </summary>
    public static ParsedPost Parse(string html)
    {
        if (string.IsNullOrEmpty(html))
            throw new ArgumentException("HTML пустой", nameof(html));

        string json = ExtractNuxtJson(html);
        JArray root = JArray.Parse(json);

        var parser = new PostParser(root);
        return parser.ParsePost();
    }

    private ParsedPost ParsePost()
    {
        JObject postRaw = FindPost(_root);
        if (postRaw == null)
            throw new Exception("Пост не найден в __NUXT_DATA__");

        // Разрешаем ссылки один раз для всего поста
        var post = Resolve(postRaw, _root, true, new HashSet<int>()) as JObject;
        if (post == null)
            throw new Exception("Не удалось разрешить пост");

        var result = new ParsedPost
        {
            Title = post["title"]?.ToString() ?? "",
            Slug = post["slug"]?.ToString() ?? ""
        };

        if (post["document"] is JObject document &&
            document["doc"] is JObject doc &&
            doc["content"] is JArray docContent)
        {
            ExtractFromDocNodes(docContent, result.Paragraphs);
        }

        if (post["content"] is JArray blocks)
        {
            ExtractFromBlocks(blocks, result.Paragraphs);
        }

        result.Text = string.Join("\n", result.Paragraphs);
        return result;
    }
    private static JObject FindPost(JArray root)
    {
        for (int i = 0; i < root.Count; i++)
        {
            if (root[i] is JObject obj &&
                obj["title"] != null &&
                obj["slug"] != null &&
                (obj["document"] != null || obj["content"] != null))
            {
                return obj;
            }
        }
        return null;
    }

    private static void ExtractFromDocNodes(JToken token, List<string> texts)
    {
        if (token == null) return;

        if (token is JObject obj)
        {
            if (obj["text"] is JValue tv && tv.Type == JTokenType.String)
            {
                string t = tv.Value<string>();
                if (!string.IsNullOrWhiteSpace(t))
                    texts.Add(t);
                return;
            }

            foreach (var prop in obj.Properties())
                ExtractFromDocNodes(prop.Value, texts);
        }
        else if (token is JArray arr)
        {
            foreach (var item in arr)
                ExtractFromDocNodes(item, texts);
        }
    }
    private static void ExtractFromBlocks(JArray blocks, List<string> texts)
    {
        foreach (var blockTok in blocks)
        {
            if (blockTok is not JObject block) continue;

            string blockType = block["block"]?.ToString();
            var json = block["json"] as JObject;

            if (json?["content"] is JValue contentVal &&
                contentVal.Type == JTokenType.String)
            {
                string plain = StripHtml(contentVal.Value<string>());
                if (!string.IsNullOrWhiteSpace(plain))
                    texts.Add(plain);
            }

            // Блок-галерея: собираем подписи к картинкам
            if (blockType == "gallery" && json?["images"] is JArray imgs)
            {
                foreach (var img in imgs)
                {
                    if (img is JObject imgObj &&
                        imgObj["caption"] is JValue cap &&
                        cap.Type == JTokenType.String &&
                        !string.IsNullOrWhiteSpace(cap.Value<string>()))
                    {
                        texts.Add("[" + cap.Value<string>() + "]");
                    }
                }
            }
        }
    }

    public static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";

        html = Regex.Replace(html, @"<\s*br\s*/?\s*>", "\n", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"</\s*(p|div|h[1-6]|li|tr)\s*>", "\n", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<\s*li[^>]*>", "• ", RegexOptions.IgnoreCase);

        html = Regex.Replace(html, "<.*?>", "", RegexOptions.Singleline);

        html = html.Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Replace("&quot;", "\"")
                   .Replace("&#39;", "'")
                   .Replace("&nbsp;", " ");

        html = Regex.Replace(html, @"[ \t]+\n", "\n");
        html = Regex.Replace(html, @"\n{3,}", "\n\n");

        return html.Trim();
    }

    private static string ExtractNuxtJson(string html)
    {
        var m = Regex.Match(
            html,
            @"<script[^>]*id=[""']__NUXT_DATA__[""'][^>]*>(.*?)</script>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (!m.Success)
            throw new Exception("__NUXT_DATA__ не найден в HTML");

        return m.Groups[1].Value.Trim();
    }
    private static JToken Resolve(JToken token, JArray root, bool isRootElement, HashSet<int> stack)
    {
        if (token is JValue v && v.Type == JTokenType.Integer && !isRootElement)
        {
            int idx = v.Value<int>();
            if (idx < 0 || idx >= root.Count) return JValue.CreateNull();
            if (!stack.Add(idx)) return JValue.CreateNull();

            var res = Resolve(root[idx], root, true, stack);
            stack.Remove(idx);
            return res;
        }

        if (token is JObject obj)
        {
            var n = new JObject();
            foreach (var p in obj.Properties())
                n[p.Name] = Resolve(p.Value, root, false, stack);
            return n;
        }

        if (token is JArray arr)
        {
            var n = new JArray();
            foreach (var it in arr) n.Add(Resolve(it, root, false, stack));
            return n;
        }

        return token;
    }
}