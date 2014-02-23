using MyWorldIsComics.DataModel.ResponseSchemas;

namespace MyWorldIsComics.DataModel.Interfaces
{
    public interface IResponse
    {
        string Aliases { get; set; }
        string Api_Detail_Url { get; set; }
        string Deck { get; set; }
        string Description { get; set; }
        int Id { get; set; }
        Images Image { get; set; }
         string Name { get; set; }
        Publisher Publisher { get; set; }
        string Resource_Type { get; set; }
        string Site_Detail_Url { get; set; }
    }
}
