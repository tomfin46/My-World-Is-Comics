namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using Interfaces;

    using MyWorldIsComics.DataModel.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

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

        #endregion

        public int FirstAppearanceId { get; set; }
        public Issue FirstAppearanceIssue { get; set; }
        public List<int> IssueIds { get; set; }
        public ObservableCollection<Issue> Issues { get; set; }
        public int IssueAppearancesCount
        {
            get
            {
                return IssueIds.Count;
            }
        }

        public StoryArc()
        {
            IssueIds = new List<int>();
            Issues = new ObservableCollection<Issue>();
        }
    }
}
