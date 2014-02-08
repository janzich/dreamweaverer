using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using MoreLinq;

namespace DreamWeaverer
{
    public static class Utils
    {

        public static void RemoveTagsByName(HtmlNode htmlNode, string tagName)
        {
            foreach (HtmlNode node in htmlNode.Descendants(tagName).ToList())
            {
                node.Remove();
            }
        }

        public static string RemoveHtmlCommentMarks(string comment)
        {

            if (comment == null)
            {
                return null;
            }

            string commentContent = comment.Trim();
            if (commentContent.StartsWith("<!--") && commentContent.EndsWith("-->"))
            {
                return commentContent.
                    RemoveFromStart("<!--").
                    RemoveFromEnd("-->").
                    Trim();
            }

            return comment;

        }

        public static string CombineUrls(string baseUrl, string url)
        {

            Stack<string> components = new Stack<string>();

            foreach (string component in Enumerable.Concat(baseUrl.Explode('/'), url.Explode('/')))
            {
                if (component == ".." && components.Count > 0)
                {
                    components.Pop();
                }
                else if (component != ".")
                {
                    components.Push(component);
                }
            }

            return components.Reverse().ConcatenateString("/");

        }

        public static string MakeRelativeUrl(string baseUrl, string url)
        {

            List<string> baseUrlComponents = baseUrl.Explode('/');
            List<string> urlComponents = url.Explode('/');

            int i = 0;
            while (
                i < baseUrlComponents.Count &&
                i < urlComponents.Count &&
                baseUrlComponents[i] == urlComponents[i])
            {
                i++;
            }

            return
                Enumerable.Concat(
                    Enumerable.Repeat("..", baseUrlComponents.Count - i),
                    urlComponents.Skip(i)).
                ConcatenateString("/");

        }

        public static string PathCombine(string baseDir, IEnumerable<string> components)
        {
            return Path.Combine(new string[] { baseDir }.Concat(components).ToArray());
        }

        public static IEnumerable<List<string>> EnumerateFiles(string baseDir, List<string> parentDirComponents = null)
        {

            if (parentDirComponents == null)
            {
                parentDirComponents = new List<string>();
            }

            string dirPath = Utils.PathCombine(baseDir, parentDirComponents);

            foreach (string filePath in Directory.GetFiles(dirPath))
            {
                yield return parentDirComponents.Concat(Path.GetFileName(filePath)).ToList();
            }

            foreach (string subDirPath in Directory.GetDirectories(dirPath))
            {
                parentDirComponents.Add(Path.GetFileName(subDirPath));
                foreach (List<string> path in EnumerateFiles(baseDir, parentDirComponents))
                {
                    yield return path;
                }
                parentDirComponents.RemoveAt(parentDirComponents.Count - 1);
            }

        }

    }
}
