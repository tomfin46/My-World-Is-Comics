using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Enums;

    public class Publisher : IResource
    {
        #region IResource Fields

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
                Uri uri = default(Uri);
                if (MainImage != null)
                {
                    uri =
                        new Uri(
                            MainImage.AbsoluteUri.Replace(
                                ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.ScaleLarge),
                                ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.SquareAvatar)));
                }

                return uri;
            }
        }

        public string PublisherName { get; set; }
        public List<string> Aliases { get; set; }
        public string AliasesString
        {
            get
            {
                return Aliases.Count == 1 && Aliases.First() == String.Empty ? "None" : string.Join(", ", Aliases);
            }
        }
        public RealLifeLocation Location { get; set; }

        #endregion

        public Publisher()
        {
            Aliases = new List<string>();
            Location = new RealLifeLocation();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
