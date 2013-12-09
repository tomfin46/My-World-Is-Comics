using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel.Interfaces
{
    using System;

    interface IResource
    {
        int UniqueId { get; set; }
        string Name { get; set; }
        Uri ComicVineApiUrl { get; set; }
        Uri ComicVineSiteUrl { get; set; }
        string Deck { get; set; }
        string DescriptionString { get; set; }
        Uri MainImage { get; set; }
        Uri AvatarImage { get; }
        string ToString();
    }
}
