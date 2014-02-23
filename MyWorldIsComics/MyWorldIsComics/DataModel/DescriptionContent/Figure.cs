using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
    class Figure : IDescriptionContent
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Uri ImageSource { get; set; }
        public Queue<IDescriptionContent> ContentQueue { get; set; }
        public List<Link> Links { get; set; }

        public Figure()
        {
            ContentQueue = new Queue<IDescriptionContent>();
            Links = new List<Link>();
        }
    }
}
