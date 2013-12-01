namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using Interfaces;

    using MyWorldIsComics.DataModel.Enums;

    public class StoryArc : IResource
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
                return new Uri(MainImage.AbsoluteUri.Replace(ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.ScaleLarge), ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.SquareAvatar)));
            }
        }

        #endregion
    }
}
