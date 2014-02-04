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
    
    }
}
