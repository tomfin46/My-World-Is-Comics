using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.Resources
{
    using System;

    using MyWorldIsComics.DataModel.Enums;
    using MyWorldIsComics.DataModel.Interfaces;
    using System.Collections.Generic;

    public class Issue : IResource
    {
        #region IResource Fields

        public int UniqueId { get; set; }
        public string Name { get; set; }

        public string IssueTitle
        {
            get
            {
                return Name == String.Empty ? "<No Title>" : Name;
            }
        }

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

        #region Image Specific Fields
        public int IssueNumber { get; set; }
        public string IssueNumberString
        {
            get
            {
                return " #" + IssueNumber;
            }
        }
        public string ResourceString { get; set; }
        public DateTime CoverDate { get; set; }
        public string CoverDateString
        {
            get
            {
                return CoverDate == default(DateTime) ? "Unknown" : CoverDate.ToString("MMM yyyy");
            }
        }
        public DateTime StoreDate { get; set; }
        public string StoreDateString
        {
            get
            {
                return StoreDate == default(DateTime) ? "Unknown" : StoreDate.ToString("d MMM yyyy");
            }
        }
        public int StaffReviewId { get; set; }
        public int VolumeId { get; set; }
        public string VolumeName { get; set; }

        #endregion

        #region Collections

        public List<int> CharacterIds { get; set; }
        public ObservableCollection<Character> Characters { get; set; }
        public List<int> ConceptIds { get; set; }
        public List<int> LocationIds { get; set; }
        public List<int> ObjectIds { get; set; }
        public ObservableCollection<Creator> Creators { get; set; }
        public Dictionary<int, string> PersonIds { get; set; }
        public List<int> StoryArcIds { get; set; }
        public List<int> TeamIds { get; set; }

        #endregion

        public Issue()
        {
            CharacterIds = new List<int>();
            Characters = new ObservableCollection<Character>();
            ConceptIds = new List<int>();
            LocationIds = new List<int>();
            ObjectIds = new List<int>();
            Creators = new ObservableCollection<Creator>();
            PersonIds = new Dictionary<int, string>();
            StoryArcIds = new List<int>();
            TeamIds = new List<int>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
