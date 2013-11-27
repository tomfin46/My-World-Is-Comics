using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel.Interfaces
{
    internal interface IDescriptionContent
    {
        string Title { get; set; }
        string Text { get; set; }
        Uri ImageSource { get; set; }
        Queue<IDescriptionContent> ContentQueue { get; set; }
        List<Link> Links { get; set; }
    }

}
