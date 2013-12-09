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

        #region Issue Specific Fields
        public int IssueNumber { get; set; }
        public string IssueNumberString
        {
            get
            {
                return "#" + IssueNumber;
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
        public ObservableCollection<Concept> Concepts { get; set; }
        public List<int> LocationIds { get; set; }
        public ObservableCollection<Location> Locations { get; set; }
        public List<int> ObjectIds { get; set; }
        public ObservableCollection<ObjectResource> Objects { get; set; }
        public Dictionary<int, string> PersonIds { get; set; }
        public ObservableCollection<Creator> Creators { get; set; }
        public List<int> StoryArcIds { get; set; }
        public ObservableCollection<StoryArc> StoryArcs { get; set; }
        public List<int> TeamIds { get; set; }
        public ObservableCollection<Team> Teams { get; set; }

        #endregion

        public Issue()
        {
            CharacterIds = new List<int>();
            Characters = new ObservableCollection<Character>();
            ConceptIds = new List<int>();
            Concepts = new ObservableCollection<Concept>();
            LocationIds = new List<int>();
            Locations = new ObservableCollection<Location>();
            ObjectIds = new List<int>();
            Objects = new ObservableCollection<ObjectResource>();
            PersonIds = new Dictionary<int, string>();
            Creators = new ObservableCollection<Creator>();
            StoryArcIds = new List<int>();
            StoryArcs = new ObservableCollection<StoryArc>();
            TeamIds = new List<int>();
            Teams = new ObservableCollection<Team>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
