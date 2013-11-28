using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
    using System.Linq;

    class Paragraph : IDescriptionContent
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Uri ImageSource { get; set; }
        public Queue<IDescriptionContent> ContentQueue { get; set; }
        public List<Link> Links { get; set; }

        public Paragraph()
        {
            ContentQueue = new Queue<IDescriptionContent>();
        }

        public string FormatLinks()
        {
            var text = Text;
            foreach (Link link in Links)
            {
                var hyperlink = "<Hyperlink NavigateUri=\"http://www.comicvine.com" + link.Href + "\">" + link.Text + "</Hyperlink>";
                Link linkCopy = link;
                Boolean contains = false;
                foreach (Link link2 in Links.Where(link1 => link1.Text != linkCopy.Text))
                {
                    if (link2.Text.Contains(link.Text))
                    {
                        contains = true;
                    }
                }

                if (!contains)
                {
                    text = text.Replace(link.Text, hyperlink);
                }
            }
            return text;
        }
    }
}