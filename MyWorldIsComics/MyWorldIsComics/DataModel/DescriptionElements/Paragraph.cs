using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.DescriptionElements
{
    class Paragraph
    {
        public string Text { get; set; }
        public List<Link> Links { get; set; }

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