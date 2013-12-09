﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
    class List : IDescriptionContent
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Uri ImageSource { get; set; }
        public Queue<IDescriptionContent> ContentQueue { get; set; }
        public List<Link> Links { get; set; }

        public List()
        {
            ContentQueue = new Queue<IDescriptionContent>();
            Links = new List<Link>();
        }
    }
}
