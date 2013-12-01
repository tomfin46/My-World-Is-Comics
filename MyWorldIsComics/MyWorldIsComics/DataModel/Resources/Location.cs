using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
    public class Location : IResource
    {
        public int UniqueId { get; set; }
        public string Name { get; set; }
        public Uri ComicVineApiUrl { get; set; }
        public Uri ComicVineSiteUrl { get; set; }
        public string Deck { get; set; }
        public string DescriptionString { get; set; }
        public Uri MainImage { get; set; }
        public Uri AvatarImage
        {
            get
            {
                return new Uri(MainImage.AbsoluteUri.Replace(ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.ScaleLarge), ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.SquareAvatar)));
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
