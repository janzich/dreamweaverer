using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace DreamWeaverer
{
    class PageRegion
    {

        private HtmlCommentNode startComment;
        private HtmlCommentNode endComment;
        private IReadOnlyList<HtmlNode> nodes;
        private IReadOnlyList<KeyValuePair<string, string>> options;

        public PageRegion(
            HtmlCommentNode startComment,
            HtmlCommentNode endComment,
            IReadOnlyList<HtmlNode> nodes,
            IReadOnlyList<KeyValuePair<string, string>> options)
        {
            this.startComment = startComment;
            this.endComment = endComment;
            this.nodes = nodes;
            this.options = options;
        }

        public HtmlCommentNode StartComment
        {
            get
            {
                return startComment;
            }
        }

        public HtmlCommentNode EndComment
        {
            get
            {
                return endComment;
            }
        }

        public IReadOnlyList<HtmlNode> Nodes
        {
            get
            {
                return nodes;
            }
        }

        public IReadOnlyList<KeyValuePair<string, string>> Options
        {
            get
            {
                return options;
            }
        }

        public string GetOptionValue(string key)
        {
            return options.
                Where(o => o.Key == key).
                Select(o => o.Value).
                FirstOrDefault();
        }

        public string Name
        {
            get
            {
                return GetOptionValue("name");
            }
        }

        public bool HasName
        {
            get
            {
                return Name != null;
            }
        }

        public static string CreateMark(string regioName, IEnumerable<KeyValuePair<string, string>> options = null)
        {

            string optionsStr = options == null || !options.Any() ? null :
                options.Select(o => string.Format("{0}=\"{1}\"", o.Key, o.Value)).ConcatenateString(" ");

            return "<!-- " + regioName + (optionsStr != null ? " " + optionsStr : "") + " -->";
        
        }
    
    }
}
