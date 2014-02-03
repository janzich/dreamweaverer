using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace DreamWeaverer
{
    class Program
    {

        private static readonly Regex regexRegionName = new Regex("name\\s*=\\s*\"(?<name>[^\"]+)\"");

        class Region
        {
            public HtmlCommentNode Start;
            public HtmlCommentNode End;
            public List<HtmlNode> Nodes = new List<HtmlNode>();
            public string Name;
        }

        private static string RemoveHtmlCommentMarks(string comment)
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

        private static List<Region> FindRegions(HtmlNode node, string startRegionMark, string endRegionMark, List<Region> regions = null)
        {

            if (regions == null)
            {
                regions = new List<Region>();
            }

            if (node != null && node.HasChildNodes)
            {

                Region currentRegion = null;

                foreach (HtmlNode childNode in node.ChildNodes)
                {

                    if (childNode.NodeType == HtmlNodeType.Comment)
                    {
                        string comment = RemoveHtmlCommentMarks(((HtmlCommentNode)childNode).Comment);
                        if (currentRegion == null)
                        {
                            if (comment.StartsWith(startRegionMark))
                            {
                                Match regionNameMatch = regexRegionName.Match(comment);
                                if (regionNameMatch.Success)
                                {
                                    currentRegion = new Region();
                                    currentRegion.Name = regionNameMatch.Groups["name"].Value;
                                    currentRegion.Start = (HtmlCommentNode)childNode;
                                }
                            }
                        }
                        else
                        {
                            if (comment.StartsWith(endRegionMark))
                            {
                                currentRegion.End = (HtmlCommentNode)childNode;
                                regions.Add(currentRegion);
                                currentRegion = null;
                            }
                        }
                    }
                    else if (currentRegion != null)
                    {
                        currentRegion.Nodes.Add(childNode);
                    }
                    else
                    {
                        FindRegions(childNode, startRegionMark, endRegionMark, regions);
                    }

                }

            }

            return regions;

        }

        private static void TranslateUrls(HtmlDocument htmlDoc, string oldPath, string newPath)
        {

            Tuple<string, string> relPaths = Utils.RemoveCommonPrefix(
                Path.GetDirectoryName(newPath),
                Path.GetDirectoryName(oldPath));

            string fromNewToOldPrefix =
                Enumerable.Concat(
                    relPaths.Item1.Explode('\\').Select(c => ".."),
                    relPaths.Item2.Explode('\\')).
                ConcatenateString("/");

            if (!fromNewToOldPrefix.IsNullOrEmpty())
            {
                fromNewToOldPrefix += "/";
            }

            foreach (HtmlNode node in htmlDoc.DocumentNode.Descendants())
            {
                if (node.Name == "img")
                {
                    PrefixUrlAttribute(node, "src", fromNewToOldPrefix);
                }
                if (node.Name == "a")
                {
                    PrefixUrlAttribute(node, "href", fromNewToOldPrefix);
                }
            }

        }

        private static void PrefixUrlAttribute(HtmlNode node, string attrName, string prefix)
        {
            if (node != null)
            {
                string url = node.GetAttributeValue(attrName, null);
                if (!url.IsNullOrWhitespace())
                {
                    url = url.Trim();
                    if (!url.Contains(':') && !url.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        node.SetAttributeValue(attrName, prefix + url);
                    }
                }
            }
        }

        private static void SimplifyRelativeUrls(HtmlDocument htmlDoc)
        {

            foreach (HtmlNode node in htmlDoc.DocumentNode.Descendants())
            {
                if (node.Name == "img")
                {
                    SimplifyUrlAttribute(node, "src");
                }
                if (node.Name == "a")
                {
                    SimplifyUrlAttribute(node, "href");
                }
            }

        }

        private static void SimplifyUrlAttribute(HtmlNode node, string attrName)
        {
            if (node != null)
            {
                string url = node.GetAttributeValue(attrName, null);
                if (!url.IsNullOrWhitespace())
                {
                    url = url.Trim();
                    if (!url.Contains(':') && !url.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        node.SetAttributeValue(attrName, SimplifyRelativeUrls(url));
                    }
                }
            }
        }

        private static string SimplifyRelativeUrls(string url)
        {

            Stack<string> components = new Stack<string>();

            foreach (string component in url.Explode('/'))
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

        static void Main(string[] args)
        {

            string baseDir = @"C:\Users\Jan Zich\Documents\Blomeyer\Site";
            string templateRelPath = @"Templates\content.dwt";

            foreach (string pagePath in Directory.EnumerateFiles(baseDir, "*.html", SearchOption.AllDirectories))
            {

                Console.WriteLine(pagePath);

                string templatePath = Path.Combine(baseDir, templateRelPath);

                HtmlDocument templateDoc = new HtmlDocument();
                templateDoc.Load(templatePath);

                TranslateUrls(templateDoc, templatePath, pagePath);
                // SimplifyRelativeUrls(templateDoc);

                // TODO: make all links relative to the page directory
                // 

                List<Region> templateRegions = FindRegions(templateDoc.DocumentNode, "TemplateBeginEditable", "TemplateEndEditable");

                // Console.WriteLine("TEMPLATE REGIONS:");
                // foreach (Region region in templateRegions)
                // {
                //     Console.WriteLine("    {0}: {1} node(s)", region.Name, region.Nodes.Count);
                // }

                HtmlDocument pageDoc = new HtmlDocument();
                pageDoc.Load(pagePath);

                List<Region> pageRegions = FindRegions(pageDoc.DocumentNode, "InstanceBeginEditable", "InstanceEndEditable");

                // Console.WriteLine("PAGE REGIONS:");
                // foreach (Region region in pageRegions)
                // {
                //     Console.WriteLine("    {0}: {1} node(s)", region.Name, region.Nodes.Count);
                // }

                foreach (Region templateRegion in templateRegions)
                {
                    Region pageRegion = pageRegions.FirstOrDefault(r => r.Name == templateRegion.Name);
                    if (pageRegion != null)
                    {

                        foreach (HtmlNode node in templateRegion.Nodes)
                        {
                            node.Remove();
                        }

                        StringBuilder html = new StringBuilder();
                        foreach (HtmlNode node in pageRegion.Nodes)
                        {
                            html.Append(node.OuterHtml);
                        }

                        templateRegion.Start.InsertHtmlAfter(html.ToString());

                    }
                }

                templateDoc.Save(pagePath);

            }

        }

    }
}
