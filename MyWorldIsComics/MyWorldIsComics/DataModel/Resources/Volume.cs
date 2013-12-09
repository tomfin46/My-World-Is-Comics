using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
    public class Volume : IResource
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

        #endregion

        public int FirstIssueId { get; set; }
        public int CountOfIssues { get; set; }
        public List<int> IssueIds { get; set; }
        public ObservableCollection<Issue> Issues { get; set; }
        public int PublisherId { get; set; }
        public string PublisherName { get; set; }
        public Section Description { get; set; }
        public string StartYear { get; set; }

        public Volume()
        {
            IssueIds = new List<int>();
            Issues = new ObservableCollection<Issue>();
        }
    }
}
