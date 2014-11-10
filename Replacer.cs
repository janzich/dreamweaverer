using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;
using MoreLinq;

namespace DreamweaverReplacer
{
    public class Replacer
    {

        public enum FileReplacementResultType
        {
            Success,
            Problem
        }

        public class FileReplacementResultSummary
        {

            private int fileIndex;
            private int fileCount;
            private string relFilePath;
            private FileReplacementResultType type;
            private string message;

            public FileReplacementResultSummary(int fileIndex, int fileCount, string relFilePath, FileReplacementResultType type, string message)
            {
                this.fileIndex = fileIndex;
                this.fileCount = fileCount;
                this.relFilePath = relFilePath;
                this.type = type;
                this.message = message;
            }

            public int FileIndex
            {
                get
                {
                    return fileIndex;
                }
            }

            public int FileCount
            {
                get
                {
                    return fileCount;
                }
            }

            public string RelFilePath
            {
                get
                {
                    return relFilePath;
                }
            }

            public FileReplacementResultType Type
            {
                get
                {
                    return type;
                }
            }

            public string Message
            {
                get
                {
                    return message;
                }
            }

        }

        private class InsertLibrariesResult
        {

            private int validLibraries;
            private int invalidLibraries;

            public InsertLibrariesResult(int validLibraries, int invalidLibraries)
            {
                this.validLibraries = validLibraries;
                this.invalidLibraries = invalidLibraries;
            }

            public int ValidLibraries
            {
                get
                {
                    return validLibraries;
                }
            }

            public int InvalidLibraries
            {
                get
                {
                    return invalidLibraries;
                }
            }

        }

        private class ApplyTemplateResult
        {

            private bool hadTemplate;
            private bool templateFound;
            private HtmlDocument htmlDocument;
            private int validRegions;
            private int invalidRegions;

            public ApplyTemplateResult(HtmlDocument htmlDocument, bool hadTemplate, bool templateFound, int validRegions, int invalidRegions)
            {
                this.hadTemplate = hadTemplate;
                this.templateFound = templateFound;
                this.htmlDocument = htmlDocument;
                this.validRegions = validRegions;
                this.invalidRegions = invalidRegions;
            }

            public bool HadTemplate
            {
                get
                {
                    return hadTemplate;
                }
            }

            public bool TemplateFound
            {
                get
                {
                    return templateFound;
                }
            }

            public HtmlDocument HtmlDocument
            {
                get
                {
                    return htmlDocument;
                }
            }

            public int ValidRegions
            {
                get
                {
                    return validRegions;
                }
            }

            public int InvalidRegions
            {
                get
                {
                    return invalidRegions;
                }
            }

        }

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

        private static ApplyTemplateResult ApplyTemplate(HtmlDocument pageDoc, string baseDir, List<string> pageBaseRelPath)
        {

            // find template name in it
            List<PageRegion> instanceRegions = FindRegions(pageDoc.DocumentNode, "InstanceBegin", "InstanceEnd");

            // no template, skip
            if (!instanceRegions.Any())
            {
                return new ApplyTemplateResult(pageDoc, false, false, 0, 0);
            }

            // there should be only one <!-- InstanceBegin template="/Templates/content.dwt" ... -->
            PageRegion firstInstanceRegion = instanceRegions.First();

            // template name
            string templateNameStr = firstInstanceRegion.GetOptionValue("template");

            // find template name in it
            List<string> templBaseRelPath = templateNameStr.Explode('/');

            // actual full physical path to the template
            string templatePath = Utils.PathCombine(baseDir, templBaseRelPath);

            // load template
            string templateHtml = Utils.SafeReadAllText(templatePath);

            // not found
            if (templateHtml == null)
            {
                return new ApplyTemplateResult(pageDoc, true, false, 0, 0);
            }

            // load and parse the template HTML
            HtmlDocument templateDoc = new HtmlDocument();
            templateDoc.LoadHtml(templateHtml);

            // change URLs in the template 
            TranslateUrls(templateDoc, templBaseRelPath, pageBaseRelPath);

            // find page regions
            List<PageRegion> pageRegions = FindRegions(pageDoc.DocumentNode, "InstanceBeginEditable", "InstanceEndEditable");

            // find template regions
            List<PageRegion> templateRegions = FindRegions(templateDoc.DocumentNode, "TemplateBeginEditable", "TemplateEndEditable");

            // for reporting
            int validRegions = 0;
            int invalidRegions = 0;

            // replace the template regions with content from the page
            foreach (PageRegion templateRegion in templateRegions)
            {

                // find regions in the source document
                PageRegion pageRegion = pageRegions.FirstOrDefault(r => r.Name == templateRegion.Name);

                // not found
                if (pageRegion == null)
                {
                    invalidRegions++;
                    continue;
                }

                // remove all content in the template in this region
                foreach (HtmlNode node in templateRegion.Nodes)
                {
                    node.Remove();
                }

                // get content from the source document for that region
                StringBuilder html = new StringBuilder();
                foreach (HtmlNode node in pageRegion.Nodes)
                {
                    html.Append(node.OuterHtml);
                }

                // place it in
                templateRegion.StartComment.InsertHtmlAfter(html.ToString());

                // preserve comments
                templateRegion.StartComment.Comment = PageRegion.CreateMark("InstanceBeginEditable", pageRegion.Options);
                templateRegion.EndComment.Comment = PageRegion.CreateMark("InstanceEndEditable");

                // all good
                validRegions++;

            }

            // store template name in the page the same way as Dreamweaver
            StoreTemplateName(templateDoc, instanceRegions.First().Options);

            // done
            return new ApplyTemplateResult(templateDoc, true, true, validRegions, invalidRegions);

        }

        private static InsertLibrariesResult InsertLibraries(HtmlDocument pageDoc, string baseDir, List<string> pageBaseRelPath)
        {

            // for reporting
            int validLibraries = 0;
            int invalidLibraries = 0;

            // find library regions: <!-- #BeginLibraryItem "/Library/buzz.lbi" --> ... <!-- #EndLibraryItem -->
            List<PageRegion> libRegions = FindRegions(pageDoc.DocumentNode, "#BeginLibraryItem", "#EndLibraryItem");

            // process each
            foreach (PageRegion libRegion in libRegions)
            {

                // there has to be an option with the library name, it's the first one and it has no key
                string libRelPathStr = libRegion.GetOptionValue(null);

                // no library
                if (libRelPathStr == null)
                {
                    invalidLibraries++;
                    continue;
                }

                // base relative path
                List<string> libBaseRelPath = libRelPathStr.Explode('/');

                // actual physical path
                string libPath = Utils.PathCombine(baseDir, libBaseRelPath);

                // load library
                string libHtml = Utils.SafeReadAllText(libPath);

                // does not exist
                if (libHtml == null)
                {
                    invalidLibraries++;
                    continue;
                }

                // load
                HtmlDocument libDoc = new HtmlDocument();
                libDoc.LoadHtml(libHtml);

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
                validLibraries++;

            }

            return new InsertLibrariesResult(validLibraries, invalidLibraries);

        }

        private static FileReplacementResultSummary CreateLogSummary(
            int fileIndex, int fileCount, List<string> pageBaseRelPath,
            ApplyTemplateResult applyTemplateResult, InsertLibrariesResult insertLibrariesResult)
        {

            bool problem = false;

            string templateMessage;
            if (!applyTemplateResult.HadTemplate)
            {
                templateMessage = "No template.";
            }
            else
            {
                if (!applyTemplateResult.TemplateFound)
                {
                    problem = true;
                    templateMessage =
                        "Template not found.";
                }
                else if (applyTemplateResult.ValidRegions == 0 && applyTemplateResult.InvalidRegions == 0)
                {
                    templateMessage =
                        "Template with no regions (nothing to do).";
                }
                else if (applyTemplateResult.InvalidRegions > 0)
                {
                    problem = true;
                    templateMessage = string.Format(
                        "Some template regions invalid ({0}).",
                        applyTemplateResult.InvalidRegions);
                }
                else
                {
                    templateMessage = string.Format(
                        "All template regions successfully updated ({0}).",
                        applyTemplateResult.ValidRegions);
                }
            }

            string librariesMessage;
            if (insertLibrariesResult.InvalidLibraries == 0 && insertLibrariesResult.ValidLibraries == 0)
            {
                librariesMessage =
                    "No libraries found.";
            }
            else if (insertLibrariesResult.InvalidLibraries > 0)
            {
                problem = true;
                librariesMessage = string.Format(
                    "Some libraries invalid ({0}).",
                    insertLibrariesResult.InvalidLibraries);
            }
            else
            {
                librariesMessage = string.Format(
                    "All libraries successfully replaced ({0}).",
                    insertLibrariesResult.ValidLibraries);
            }

            return new FileReplacementResultSummary(
                fileIndex, fileCount,
                Path.Combine(pageBaseRelPath.ToArray()),
                problem ? FileReplacementResultType.Problem : FileReplacementResultType.Success,
                templateMessage + " " + librariesMessage);

        }

        public static void Replace(string baseDir, Action<FileReplacementResultSummary> fileDone)
        {

            List<List<string>> files =
                Utils.EnumerateFiles(baseDir).
                Where(p => Path.GetExtension(p.Last()).OneOfInvariantIgnoreCase(".html")).
                ToList();

            int fileIdx = 0;
            foreach (List<string> pageBaseRelPath in files)
            {

                // full file path
                string pagePath = Utils.PathCombine(baseDir, pageBaseRelPath);

                // load and parse the page HTML
                HtmlDocument pageDoc = new HtmlDocument();
                pageDoc.Load(pagePath);

                // apply template
                ApplyTemplateResult applyTemplateResult = ApplyTemplate(pageDoc, baseDir, pageBaseRelPath);
                pageDoc = applyTemplateResult.HtmlDocument;

                // insert libraries
                InsertLibrariesResult insertLibrariesResult = InsertLibraries(pageDoc, baseDir, pageBaseRelPath);

                // done
                pageDoc.Save(pagePath);

                // log
                if (fileDone != null)
                {
                    fileDone(CreateLogSummary(
                        fileIdx, files.Count, pageBaseRelPath,
                        applyTemplateResult, insertLibrariesResult));
                }

            }

        }

    }
}
