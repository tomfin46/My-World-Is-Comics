namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Interfaces;

    using MyWorldIsComics.DataModel.Enums;

    public class Team : IResource
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

        #region Team Specific Fields

        public int FirstAppearanceId { get; set; }
        public Issue FirstAppearanceIssue { get; set; }
        public int IssueAppearancesCount { get; set; }
        public int MembersCount { get; set; }
        public int PublisherId { get; set; }
        public string ResourceString { get; set; }

        #region Collections

        public List<String> Aliases { get; set; }
        public ObservableCollection<Character> Members { get; set; }
        public List<int> MemberIds { get; set; }
        public ObservableCollection<Character> Enemies { get; set; }
        public List<int> EnemyIds { get; set; }
        public ObservableCollection<Character> Friends { get; set; }
        public List<int> FriendIds { get; set; }
        public ObservableCollection<Issue> IssuesDispandedIn { get; set; }
        public List<int> IssuesDispandedInIds { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public List<int> MovieIds { get; set; }

        #endregion

        #endregion

        public Team()
        {
            Aliases = new List<string>();
            EnemyIds = new List<int>();
            Enemies = new ObservableCollection<Character>();
            FriendIds = new List<int>();
            Friends = new ObservableCollection<Character>();
            MemberIds = new List<int>();
            Members = new ObservableCollection<Character>();
            IssuesDispandedInIds = new List<int>();
            IssuesDispandedIn = new ObservableCollection<Issue>();
            MovieIds = new List<int>();
            Movies = new ObservableCollection<Movie>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
