using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Hikaria.QC.Utilities;

public class TMPTagCloser
{
    // 定义TMP支持的标签类型
    private static readonly HashSet<string> SupportedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "b", "i", "u", "s", "mark", "color", "size", "material", "quad", "align", "alpha",
        "cspace", "font", "indent", "line-height", "line-indent", "link", "lowercase",
        "uppercase", "smallcaps", "margin", "noparse", "nobr", "page", "pos", "space",
        "sprite", "style", "voffset", "width", "gradient"
    };

    // 不需要闭合的自闭合标签
    private static readonly HashSet<string> SelfClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "br", "nobr", "page", "sprite", "quad"
    };

    public static string CloseTMPTags(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 用于跟踪打开的标签
        Stack<string> openTags = new Stack<string>();

        // 使用StringBuilder构建结果
        StringBuilder result = new StringBuilder(input);

        // 查找所有标签
        string pattern = @"<([/]?)([a-z]+)(?:=([^<>]*))?>";
        MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string isClosing = match.Groups[1].Value; // 是否为闭合标签 "/"
            string tagName = match.Groups[2].Value.ToLower(); // 标签名

            // 检查是否是支持的标签
            if (!SupportedTags.Contains(tagName) && !SelfClosingTags.Contains(tagName))
                continue;

            // 如果是自闭合标签，跳过
            if (SelfClosingTags.Contains(tagName))
                continue;

            if (isClosing == "/")
            {
                // 闭合标签
                if (openTags.Count > 0 && openTags.Peek() == tagName)
                {
                    openTags.Pop();
                }
            }
            else
            {
                // 开放标签
                openTags.Push(tagName);
            }
        }

        // 为所有未闭合的标签添加闭合标签
        while (openTags.Count > 0)
        {
            string tagToClose = openTags.Pop();
            result.Append($"</{tagToClose}>");
        }

        return result.ToString();
    }

    // 更复杂的版本，处理嵌套标签的正确顺序
    public static string CloseTMPTagsAdvanced(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 使用正则表达式找出所有标签
        string pattern = @"<([/]?)([a-z]+)(?:=([^<>]*))?>";

        // 用于存储标签及其位置信息
        List<TagInfo> tags = new List<TagInfo>();
        MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            bool isClosing = match.Groups[1].Value == "/";
            string tagName = match.Groups[2].Value.ToLower();

            // 检查是否是支持的标签
            if (!SupportedTags.Contains(tagName) && !SelfClosingTags.Contains(tagName))
                continue;

            // 如果是自闭合标签，跳过
            if (SelfClosingTags.Contains(tagName))
                continue;

            tags.Add(new TagInfo
            {
                Name = tagName,
                IsClosing = isClosing,
                Position = match.Index,
                Length = match.Length
            });
        }

        // 处理标签栈
        Stack<TagInfo> openTagsStack = new Stack<TagInfo>();
        List<TagInfo> tagsToAdd = new List<TagInfo>();

        foreach (var tag in tags)
        {
            if (tag.IsClosing)
            {
                // 找到匹配的开标签
                bool found = false;
                Stack<TagInfo> tempStack = new Stack<TagInfo>();

                while (openTagsStack.Count > 0)
                {
                    var openTag = openTagsStack.Pop();
                    if (openTag.Name == tag.Name)
                    {
                        found = true;
                        break;
                    }
                    tempStack.Push(openTag);
                }

                // 如果没找到匹配的开标签，忽略这个闭标签
                if (!found)
                {
                    // 恢复栈
                    while (tempStack.Count > 0)
                    {
                        openTagsStack.Push(tempStack.Pop());
                    }
                }
                else
                {
                    // 为临时弹出的标签添加闭标签，然后再添加它们的开标签
                    int insertPosition = tag.Position;

                    while (tempStack.Count > 0)
                    {
                        var poppedTag = tempStack.Pop();

                        // 添加闭标签
                        tagsToAdd.Add(new TagInfo
                        {
                            Name = poppedTag.Name,
                            IsClosing = true,
                            Position = insertPosition,
                            Length = 0, // 这是要添加的新标签
                            IsNew = true
                        });

                        // 在闭合标签后重新添加开标签
                        tagsToAdd.Add(new TagInfo
                        {
                            Name = poppedTag.Name,
                            IsClosing = false,
                            Position = tag.Position + tag.Length,
                            Length = 0, // 这是要添加的新标签
                            IsNew = true
                        });

                        // 将标签重新压入栈
                        openTagsStack.Push(poppedTag);
                    }
                }
            }
            else
            {
                // 开标签，直接压入栈
                openTagsStack.Push(tag);
            }
        }

        // 处理剩余的未闭合标签
        int endPosition = input.Length;
        while (openTagsStack.Count > 0)
        {
            var openTag = openTagsStack.Pop();
            tagsToAdd.Add(new TagInfo
            {
                Name = openTag.Name,
                IsClosing = true,
                Position = endPosition,
                Length = 0, // 这是要添加的新标签
                IsNew = true
            });
        }

        // 按位置从后往前排序，以便插入时不影响前面的位置
        tagsToAdd.Sort((a, b) => b.Position.CompareTo(a.Position));

        // 构建结果
        StringBuilder result = new StringBuilder(input);
        foreach (var tagToAdd in tagsToAdd)
        {
            if (tagToAdd.IsNew)
            {
                string tagText = tagToAdd.IsClosing ? $"</{tagToAdd.Name}>" : $"<{tagToAdd.Name}>";
                result.Insert(tagToAdd.Position, tagText);
            }
        }

        return result.ToString();
    }

    private class TagInfo
    {
        public string Name { get; set; }
        public bool IsClosing { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public bool IsNew { get; set; } = false;
    }
}
