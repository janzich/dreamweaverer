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
                        if (currentRegionStartComment == null && comment.StartsWith(startRegionMark))
                        {
                            currentRegionStartComment = (HtmlCommentNode)childNode;
                            currentRegionOptions = ParseOptionsString(comment.RemoveFromStart(startRegionMark));
                            currentRegionNodes = new List<HtmlNode>();
                        }
                        else if (currentRegionStartComment != null && comment.StartsWith(endRegionMark))
                        {
                            HtmlCommentNode currentRegionEndComment = (HtmlCommentNode)childNode;
                            regions.Add(new PageRegion(currentRegionStartComment, currentRegionEndComment, currentRegionNodes, currentRegionOptions));
                            currentRegionStartComment = null;
                            currentRegionOptions = null;
                            currentRegionNodes = null;
                        }
                        else if (currentRegionNodes != null)
                        {
                            currentRegionNodes.Add(childNode);
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

        private static List<KeyValuePair<string, string>> ParseOptionsString(string optionsStr)
        {

            Regex regexOption = new Regex("\\s*((?<key>[^=]+)\\s*=\\s*)?\"(?<value>[^\"]+)\"", RegexOptions.ExplicitCapture);

            List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();

            foreach (Match match in regexOption.Matches(optionsStr))
            {

                string key = match.Groups["key"].Success ? match.Groups["key"].Value : null;
                string value = match.Groups["value"].Value;

                options.Add(new KeyValuePair<string, string>(key, value));

            }

            return options;

        }

        private static void StoreTemplateName(HtmlDocument htmlDoc, IReadOnlyList<KeyValuePair<string, string>> options)
        {

            HtmlNode htmlNode = htmlDoc.DocumentNode.SelectSingleNode("html");

            htmlNode.PrependChild(
                htmlNode.OwnerDocument.CreateComment(
                    PageRegion.CreateMark("InstanceBegin", options)));

            htmlNode.AppendChild(
                htmlNode.OwnerDocument.CreateComment(
                    PageRegion.CreateMark("InstanceEnd")));

        }

        private static HtmlDocument ApplyTemplate(HtmlDocument pageDoc, string baseDir, List<string> pageBaseRelPath)
        {

            // find template name in it
            List<PageRegion> instanceRegions = FindRegions(pageDoc.DocumentNode, "InstanceBegin", "InstanceEnd");
            if (!instanceRegions.Any())
            {
                Console.WriteLine("This page does not have a template. Skipping.");
                return pageDoc;
            }

            // there should be only one <!-- InstanceBegin template="/Templates/content.dwt" ... -->
            PageRegion firstInstanceRegion = instanceRegions.First();

            // template name
            string templateNameStr = firstInstanceRegion.GetOptionValue("template");

            // find template name in it
            List<string> templBaseRelPath = templateNameStr.Explode('/');

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

                    templateRegion.StartComment.Comment = PageRegion.CreateMark("InstanceBeginEditable", pageRegion.Options);
                    templateRegion.EndComment.Comment = PageRegion.CreateMark("InstanceEndEditable");

                }
            }

            // store template name in the page the same way as Dreamweaver
            StoreTemplateName(templateDoc, instanceRegions.First().Options);

            // done
            return templateDoc;

        }

        private static void InsertLibraries(HtmlDocument pageDoc, string baseDir, List<string> pageBaseRelPath)
        {

            // find librari regions: <!-- #BeginLibraryItem "/Library/buzz.lbi" --> ... <!-- #EndLibraryItem -->
            List<PageRegion> libRegions = FindRegions(pageDoc.DocumentNode, "#BeginLibraryItem", "#EndLibraryItem");

            // process each
            foreach (PageRegion libRegion in libRegions)
            {

                // there has to be an option with the library name, it's the first one and it has no key
                string libRelPathStr = libRegion.GetOptionValue(null);

                // no library
                if (libRelPathStr == null)
                {
                    continue;
                }

                // base relative path
                List<string> libBaseRelPath = libRelPathStr.Explode('/');

                // actual physical path
                string libPath = Utils.PathCombine(baseDir, libBaseRelPath);

                // load
                HtmlDocument libDoc = new HtmlDocument();
                libDoc.Load(libPath);

                // Dreamweaver places a <meta http-equiv="Content-Type" content="..."> tag
                // at the first line of libraries, presumably to define the encoding of the file
                Utils.RemoveTagsByName(libDoc.DocumentNode, "meta");

                // translate links
                TranslateUrls(libDoc, libBaseRelPath, pageBaseRelPath);

                // remove the current content from the page
                foreach (HtmlNode node in libRegion.Nodes)
                {
                    node.Remove();
                }

                // insert the library
                libRegion.StartComment.InsertHtmlAfter(libDoc.DocumentNode.InnerHtml);

            }

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

                    // apply template
                    pageDoc = ApplyTemplate(pageDoc, baseDir, pageBaseRelPath);

                    // insert libraries
                    InsertLibraries(pageDoc, baseDir, pageBaseRelPath);

                    // done
                    pageDoc.Save(pagePath);

                }

            }

        }

    }
}
