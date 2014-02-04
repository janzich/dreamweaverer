using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;
using MoreLinq;

namespace DreamWeaverer
{
    class Program
    {

        private static List<PageRegion> FindRegions(HtmlNode node, string startRegionMark, string endRegionMark, List<PageRegion> regions = null)
        {

            if (regions == null)
            {
                regions = new List<PageRegion>();
            }

            if (node != null && node.HasChildNodes)
            {

                HtmlCommentNode currentRegionStartComment = null;
                List<KeyValuePair<string, string>> currentRegionOptions = null;
                List<HtmlNode> currentRegionNodes = null;

                foreach (HtmlNode childNode in node.ChildNodes)
                {

                    if (childNode.NodeType == HtmlNodeType.Comment)
                    {
                        string comment = Utils.RemoveHtmlCommentMarks(((HtmlCommentNode)childNode).Comment);
                        if (currentRegionStartComment == null)
                        {
                            if (comment.StartsWith(startRegionMark))
                            {
                                currentRegionStartComment = (HtmlCommentNode)childNode;
                                currentRegionOptions = ParseOptionsString(comment.RemoveFromStart(startRegionMark));
                                currentRegionNodes = new List<HtmlNode>();
                            }
                        }
                        else
                        {
                            if (comment.StartsWith(endRegionMark))
                            {
                                HtmlCommentNode currentRegionEndComment = (HtmlCommentNode)childNode;
                                regions.Add(new PageRegion(currentRegionStartComment, currentRegionEndComment, currentRegionNodes, currentRegionOptions));
                                currentRegionStartComment = null;
                                currentRegionOptions = null;
                                currentRegionNodes = null;
                            }
                        }
                    }
                    else if (currentRegionNodes != null)
                    {
                        currentRegionNodes.Add(childNode);
                    }
                    else
                    {
                        FindRegions(childNode, startRegionMark, endRegionMark, regions);
                    }

                }

            }

            return regions;

        }

        private static void TranslateUrls(HtmlDocument htmlDoc, List<string> oldPath, List<string> newPath)
        {

            string oldBaseUrl = oldPath.Take(oldPath.Count - 1).ConcatenateString("/");
            string newBaseUrl = newPath.Take(newPath.Count - 1).ConcatenateString("/");

            foreach (HtmlNode node in htmlDoc.DocumentNode.Descendants())
            {
                if (node.Name == "img")
                {
                    TranslateUrlAttribute(node, "src", oldBaseUrl, newBaseUrl);
                }
                if (node.Name == "a")
                {
                    TranslateUrlAttribute(node, "href", oldBaseUrl, newBaseUrl);
                }
            }

        }

        private static void TranslateUrlAttribute(HtmlNode node, string attrName, string oldBaseUrl, string newBaseUrl)
        {
            if (node != null)
            {
                string url = node.GetAttributeValue(attrName, null);
                if (!url.IsNullOrWhitespace())
                {
                    url = url.Trim();
                    if (!url.Contains(':') && !url.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string absUrl = Utils.CombineUrls(oldBaseUrl, url);
                        if (!absUrl.StartsWith(".."))
                        {
                            string newRelUrl = Utils.MakeRelativeUrl(newBaseUrl, absUrl);
                            node.SetAttributeValue(attrName, newRelUrl);
                        }
                    }
                }
            }
        }

        // private static void PrefixUrlAttribute(HtmlNode node, string attrName, string prefix)
        // {
        //     if (node != null)
        //     {
        //         string url = node.GetAttributeValue(attrName, null);
        //         if (!url.IsNullOrWhitespace())
        //         {
        //             url = url.Trim();
        //             if (!url.Contains(':') && !url.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
        //             {
        //                 node.SetAttributeValue(attrName, prefix + url);
        //             }
        //         }
        //     }
        // }

        // private static void SimplifyRelativeUrls(HtmlDocument htmlDoc)
        // {
        // 
        //     foreach (HtmlNode node in htmlDoc.DocumentNode.Descendants())
        //     {
        //         if (node.Name == "img")
        //         {
        //             SimplifyUrlAttribute(node, "src");
        //         }
        //         if (node.Name == "a")
        //         {
        //             SimplifyUrlAttribute(node, "href");
        //         }
        //     }
        // 
        // }
        // 
        // private static void SimplifyUrlAttribute(HtmlNode node, string attrName)
        // {
        //     if (node != null)
        //     {
        //         string url = node.GetAttributeValue(attrName, null);
        //         if (!url.IsNullOrWhitespace())
        //         {
        //             url = url.Trim();
        //             if (!url.Contains(':') && !url.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
        //             {
        //                 node.SetAttributeValue(attrName, SimplifyRelativeUrls(url));
        //             }
        //         }
        //     }
        // }
        // 
        // private static string SimplifyRelativeUrls(string url)
        // {
        // 
        //     Stack<string> components = new Stack<string>();
        // 
        //     foreach (string component in url.Explode('/'))
        //     {
        //         if (component == ".." && components.Count > 0)
        //         {
        //             components.Pop();
        //         }
        //         else if (component != ".")
        //         {
        //             components.Push(component);
        //         }
        //     }
        // 
        //     return components.Reverse().ConcatenateString("/");
        // 
        // }

        private static List<KeyValuePair<string, string>> ParseOptionsString(string optionsStr)
        {

            Regex regexOption = new Regex("\\s*(?<key>[^=]+)\\s*=\\s*\"(?<value>[^\"]+)\"");

            List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();

            foreach (Match match in regexOption.Matches(optionsStr))
            {
                options.Add(new KeyValuePair<string, string>(
                    match.Groups["key"].Value,
                    match.Groups["value"].Value));
            }

            return options;

        }

        private static List<string> FindTemplateName(HtmlDocument pageDoc)
        {

            List<PageRegion> instanceRegions = FindRegions(pageDoc.DocumentNode, "InstanceBegin", "InstanceEnd");
            if (!instanceRegions.Any())
            {
                return null;
            }

            PageRegion firstInstanceRegion = instanceRegions.First();

            string template = firstInstanceRegion.GetOptionValue("template");
            if (template == null)
            {
                return null;
            }

            return template.Explode('/');

        }

        public static void Main(string[] args)
        {

            string baseDir = @"C:\Users\Jan Zich\Documents\Blomeyer\Site";

            foreach (List<string> pageBaseRelPath in Utils.EnumerateFiles(baseDir))
            {

                string pagePath = Utils.PathCombine(baseDir, pageBaseRelPath);

                if (Path.GetExtension(pagePath).OneOfInvariantIgnoreCase(".html"))
                {

                    Console.WriteLine(pagePath);

                    // load and parse the page HTML
                    HtmlDocument pageDoc = new HtmlDocument();
                    pageDoc.Load(pagePath);

                    // find template name in it
                    List<string> templBaseRelPath = FindTemplateName(pageDoc);

                    // no template name found in the file
                    if (templBaseRelPath == null)
                    {
                        Console.WriteLine("This page does not have a template. Skipping.");
                    }

                    // template found
                    else
                    {

                        // actual full physical path to the template
                        string templatePath = Utils.PathCombine(baseDir, templBaseRelPath);

                        // load and parse the template HTML
                        HtmlDocument templateDoc = new HtmlDocument();
                        templateDoc.Load(templatePath);

                        // change URLs in the template 
                        TranslateUrls(templateDoc, templBaseRelPath, pageBaseRelPath);

                        // find page regions
                        List<PageRegion> pageRegions = FindRegions(pageDoc.DocumentNode, "InstanceBeginEditable", "InstanceEndEditable");

                        // find template regions
                        List<PageRegion> templateRegions = FindRegions(templateDoc.DocumentNode, "TemplateBeginEditable", "TemplateEndEditable");

                        // replace the template regions with content from the page
                        foreach (PageRegion templateRegion in templateRegions)
                        {
                            PageRegion pageRegion = pageRegions.FirstOrDefault(r => r.Name == templateRegion.Name);
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

                                templateRegion.StartComment.InsertHtmlAfter(html.ToString());

                                templateRegion.StartComment.Comment = "<!-- InstanceBeginEditable" + " " + templateRegion.Options.Select(o => string.Format("{0}=\"{1}\"", o.Key, o.Value)).ConcatenateString(" ") + " -->";
                                templateRegion.EndComment.Comment = "<!-- InstanceEndEditable -->";

                            }
                        }

                        // done
                        templateDoc.Save(pagePath);

                    }

                }

            }

        }

    }
}
