using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace DreamWeaverer
{
    public static class Extensions
    {

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhitespace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static List<string> Explode(this string value, params char[] separators)
        {
            return (value ?? string.Empty).Split(separators).Where(t => !t.IsNullOrWhitespace()).Select(t => t.Trim()).ToList();
        }

        public static string RemoveFromStart(this string value, string stringToRemove)
        {
            return value.RemoveFromStart(stringToRemove, StringComparison.CurrentCulture);
        }

        public static string RemoveFromStart(this string value, string stringToRemove, StringComparison comparisonType)
        {
            if (value.StartsWith(stringToRemove, comparisonType))
            {
                return value.Substring(stringToRemove.Length);
            }
            else
            {
                return value;
            }
        }

        public static string EnsureAtStart(this string value, string prefixToPrepend)
        {
            if (value.StartsWith(prefixToPrepend))
            {
                return value;
            }
            else
            {
                return prefixToPrepend + value;
            }
        }

        public static string EnsureAtStart(this string value, string prefixToPrepend, StringComparison comparisonType)
        {
            if (value.StartsWith(prefixToPrepend, comparisonType))
            {
                return value;
            }
            else
            {
                return prefixToPrepend + value;
            }
        }

        public static string EnsureAtEnd(this string value, string suffixToAppend)
        {
            if (value.EndsWith(suffixToAppend))
            {
                return value;
            }
            else
            {
                return value + suffixToAppend;
            }
        }

        public static string EnsureAtEnd(this string value, string suffixToAppend, StringComparison comparisonType)
        {
            if (value.EndsWith(suffixToAppend, comparisonType))
            {
                return value;
            }
            else
            {
                return value + suffixToAppend;
            }
        }

        public static string RemoveFromEnd(this string value, string stringToRemove)
        {
            return value.RemoveFromEnd(stringToRemove, StringComparison.CurrentCulture);
        }

        public static string RemoveFromEnd(this string value, string stringToRemove, StringComparison comparisonType)
        {
            if (value.EndsWith(stringToRemove, comparisonType))
            {
                return value.Substring(0, value.Length - stringToRemove.Length);
            }
            return value;
        }

        public static bool OneOf<T>(this T element, IEnumerable<T> list)
        {
            return list == null ? false : list.Contains(element);
        }

        public static bool OneOf<T>(this T element, IEqualityComparer<T> comparer, params T[] list)
        {
            return list == null ? false : list.Contains(element, comparer);
        }

        public static bool OneOf<T>(this T element, params T[] list)
        {
            return list == null ? false : list.Contains(element);
        }

        public static bool OneOfInvariantIgnoreCase(this string element, params string[] list)
        {
            return list == null ? false : list.Contains(element, StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool OneOfInvariantIgnoreCase(this string element, IEnumerable<string> list)
        {
            return list == null ? false : list.Contains(element, StringComparer.InvariantCultureIgnoreCase);
        }

        public static string ConcatenateString<T>(this IEnumerable<string> values, T delimiter, T finalDelimiter)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            int count = values.Count();
            foreach (string s in values)
            {
                if (i > 0 && i == count - 1)
                {
                    sb.Append(finalDelimiter);
                }
                else if (i > 0)
                {
                    sb.Append(delimiter);
                }
                sb.Append(s);
                i++;
            }
            return sb.ToString();
        }

        public static string ConcatenateString<T, S>(this IEnumerable<T> values, Func<T, string> valueToString, S delimiter, S finalDelimiter)
        {
            return values.Select(valueToString).ConcatenateString(delimiter, finalDelimiter);
        }

        public static string ConcatenateString<T, S>(this IEnumerable<T> values, Func<T, string> valueToString, S delimiter)
        {
            return values.Select(valueToString).ConcatenateString(delimiter, delimiter);
        }

        public static string ConcatenateString<T, S>(this IEnumerable<T> values, Func<T, int, string> valueAndIndexToString, S delimiter)
        {
            return values.Select(valueAndIndexToString).ConcatenateString(delimiter);
        }

        public static string ConcatenateString<T, S>(this IEnumerable<T> values, S separator)
        {
            return values.ConcatenateString(v => v.ToString(), separator);
        }

        public static void InsertHtmlAfter(this HtmlNode node, string html)
        {

            HtmlNode dummyContainer = node.OwnerDocument.CreateElement("div");
            dummyContainer.InnerHtml = html;

            if (dummyContainer.HasChildNodes)
            {
                foreach (HtmlNode childNode in dummyContainer.ChildNodes.Cast<HtmlNode>().Reverse())
                {
                    node.ParentNode.InsertAfter(childNode, node);
                }
            }

        }

    }
}
