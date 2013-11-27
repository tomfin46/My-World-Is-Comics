﻿using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
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
                text = text.Replace(link.Text, hyperlink);
            }
            return text;
        }
    }
}