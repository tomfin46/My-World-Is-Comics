using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel.Interfaces
{
    public interface IDescriptionContent
    {
        string Title { get; set; }
        string Text { get; set; }
        Uri ImageSource { get; set; }
        Queue<IDescriptionContent> ContentQueue { get; set; }
        List<Link> Links { get; set; }
    }

}
