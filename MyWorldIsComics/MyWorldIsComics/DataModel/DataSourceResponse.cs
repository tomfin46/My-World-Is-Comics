using System.Collections.Generic;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel
{
    class ComicVineResponse
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public List<IResource> Results { get; set; }
        public string Version { get; set; }
    }

    class WikiaResponse
    {
        public IList<WikiaResponseItem> Items { get; set; }
        public string BasePath { get; set; }
    }

    class WikiaResponseItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Ns { get; set; }
    }
}
