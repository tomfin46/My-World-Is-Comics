using System;
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
        public string Name { get; set; }
        public int IssueNumber { get; set; }
    }
}
