﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel
{
    class Issue
    {
        public int UniqueId { get; set; }
        public Uri ComicVineApiUrl { get; set; }
        public Uri ComicVineSiteUrl { get; set; }
        public string Title { get; set; }
        public int IssueNumber { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
