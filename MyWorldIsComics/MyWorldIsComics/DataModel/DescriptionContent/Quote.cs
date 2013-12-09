using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
    class Quote : IDescriptionContent
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Uri ImageSource { get; set; }
        public Queue<IDescriptionContent> ContentQueue { get; set; }
        public List<Link> Links { get; set; }

        public Quote()
        {
            ContentQueue = new Queue<IDescriptionContent>();
            Links = new List<Link>();
        }

        public string FormatLinks()
        {
            var text = Text;
            foreach (Link link in Links)
            {
                var hyperlink = "<Hyperlink NavigateUri=\"http://www.comicvine.com" + link.Href + "\">" + link.Text + "</Hyperlink>";
                text = text.Replace(link.Text, hyperlink);
            }
            return text;
        }
    }
}
