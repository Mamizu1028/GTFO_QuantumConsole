using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Hikaria.QC.Utilities;

public static class RichTextTagsExtensions
{
    // TMP 支持的成对标签或作用域标签
    private static readonly HashSet<string> SupportedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "b", "i", "u", "s", "mark", "color", "size", "material", "align", "alpha",
        "cspace", "font", "indent", "line-height", "line-indent", "link", "lowercase",
        "uppercase", "smallcaps", "margin", "noparse", "nobr", "pos",
        "style", "voffset", "width", "gradient"
    };

    // 不需要闭合的标签
    private static readonly HashSet<string> SelfClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "br", "page", "sprite", "quad", "space"
    };

    // 匹配普通富文本标签，例如：
    // <b>
    // </b>
    // <color=#ff0000>
    // <size=120%>
    // <line-height=120%>
    // <sprite=0>
    private static readonly Regex TagRegex = new Regex(
        @"<\s*(?<slash>/?)\s*(?<name>[a-zA-Z][a-zA-Z0-9-]*)(?:\s*=\s*[^<>]*)?\s*/?\s*>",
        RegexOptions.IgnoreCase
    );

    // 匹配 TMP 颜色简写，例如 <#fff>、<#ffffff>、<#ffffffff>
    private static readonly Regex ShortColorRegex = new Regex(
        @"<\s*#(?<hex>[0-9A-Fa-f]{3,8})\s*>",
        RegexOptions.IgnoreCase
    );

    /// <summary>
    /// 修复 TMP 富文本标签。
    /// </summary>
    public static string FixRichTextTags(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = NormalizeShortColorTags(input);

        StringBuilder result = new StringBuilder(input.Length);
        Stack<OpenTagInfo> stack = new Stack<OpenTagInfo>();

        int lastIndex = 0;
        MatchCollection matches = TagRegex.Matches(input);

        foreach (Match match in matches)
        {
            string tagName = match.Groups["name"].Value.ToLowerInvariant();
            bool isClosing = match.Groups["slash"].Value == "/";

            bool isKnownTag = SupportedTags.Contains(tagName) || SelfClosingTags.Contains(tagName);

            if (!isKnownTag)
            {
                continue;
            }

            // 追加标签之前的普通文本
            result.Append(input, lastIndex, match.Index - lastIndex);
            lastIndex = match.Index + match.Length;

            // 自闭合标签直接保留
            if (SelfClosingTags.Contains(tagName))
            {
                result.Append(match.Value);
                continue;
            }

            if (!isClosing)
            {
                // 开始标签
                result.Append(match.Value);
                stack.Push(new OpenTagInfo
                {
                    Name = tagName,
                    OpenText = match.Value
                });
            }
            else
            {
                // 结束标签
                if (stack.Count == 0)
                {
                    // 没有对应开始标签，丢弃这个闭合标签
                    continue;
                }

                if (stack.Peek().Name == tagName)
                {
                    // 正常闭合
                    stack.Pop();
                    result.Append(match.Value);
                }
                else
                {
                    // 尝试修复交叉嵌套
                    List<OpenTagInfo> temporarilyClosed = new List<OpenTagInfo>();
                    bool found = false;

                    while (stack.Count > 0)
                    {
                        OpenTagInfo openTag = stack.Pop();

                        if (openTag.Name == tagName)
                        {
                            found = true;
                            break;
                        }

                        temporarilyClosed.Add(openTag);
                    }

                    if (!found)
                    {
                        // 没找到匹配的开始标签，恢复栈并丢弃这个闭合标签
                        for (int i = temporarilyClosed.Count - 1; i >= 0; i--)
                        {
                            stack.Push(temporarilyClosed[i]);
                        }

                        continue;
                    }

                    // 先闭合临时弹出的标签
                    for (int i = 0; i < temporarilyClosed.Count; i++)
                    {
                        result.Append("</");
                        result.Append(temporarilyClosed[i].Name);
                        result.Append(">");
                    }

                    // 再闭合当前目标标签
                    result.Append(match.Value);

                    // 然后重新打开之前临时闭合的标签
                    for (int i = temporarilyClosed.Count - 1; i >= 0; i--)
                    {
                        result.Append(temporarilyClosed[i].OpenText);
                        stack.Push(temporarilyClosed[i]);
                    }
                }
            }
        }

        // 追加剩余文本
        result.Append(input, lastIndex, input.Length - lastIndex);

        // 补齐未闭合标签
        while (stack.Count > 0)
        {
            OpenTagInfo openTag = stack.Pop();
            result.Append("</");
            result.Append(openTag.Name);
            result.Append(">");
        }

        return result.ToString();
    }

    /// <summary>
    /// 移除所有已列出的 TMP 富文本标签。
    /// 只移除标签本身，不移除标签包裹的文字内容。
    /// </summary>
    public static string RemoveRichTextTags(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 先移除 TMP 颜色简写，例如 <#ff0000>
        input = ShortColorRegex.Replace(input, string.Empty);

        return TagRegex.Replace(input, match =>
        {
            string tagName = match.Groups["name"].Value;

            if (SupportedTags.Contains(tagName) || SelfClosingTags.Contains(tagName))
            {
                return string.Empty;
            }

            // 非 TMP 标签保留
            return match.Value;
        });
    }

    /// <summary>
    /// 将 TMP 颜色简写 <#ff0000> 转换为 <color=#ff0000>。
    /// </summary>
    public static string NormalizeShortColorTags(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return ShortColorRegex.Replace(input, "<color=#$1>");
    }

    private sealed class OpenTagInfo
    {
        public string Name { get; set; }
        public string OpenText { get; set; }
    }
}