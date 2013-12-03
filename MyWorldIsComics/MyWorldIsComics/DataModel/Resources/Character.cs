﻿using System.Collections.ObjectModel;
using System.Linq;

namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Interfaces;

    public class Character : IResource, IPerson
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

        #region IPerson Fields
        public DateTime Birth { get; set; }
        public string BirthString
        {
            get
            {
                return Birth == default(DateTime) ? "Unknown" : Birth.ToString("d MMM yyyy");
            }
        }

        public Gender.GenderEnum Gender { get; set; }

        #endregion

        #region Character Specific Fields

        public string RealName { get; set; }

        public string RealNameString
        {
            get
            {
                return RealName == String.Empty ? Name : RealName;
            }
        }

        public int IssueAppearancesCount { get; set; }
        public int FirstAppearanceId { get; set; }
        public Issue FirstAppearanceIssue { get; set; }
        public string Origin { get; set; }
        public int PublisherId { get; set; }
        public string AliasesString
        {
            get {
                return Aliases.Count == 1 && Aliases.First() == String.Empty ? "None" : string.Join(", ", Aliases);
            }
        }

        #region Collections

        public List<String> Aliases { get; set; }
        public List<int> EnemyIds { get; set; }
        public List<int> FriendIds { get; set; }
        public List<int> CreatorIds { get; set; }
        public List<int> DeathIssueIds { get; set; }
        public List<int> MovieIds { get; set; }
        public List<String> Powers { get; set; }
        public List<int> TeamIds { get; set; }
        public ObservableCollection<Team> Teams { get; set; }

        #endregion

        #endregion

        public Character()
        {
            Aliases = new List<string>();
            EnemyIds = new List<int>();
            FriendIds = new List<int>();
            CreatorIds = new List<int>();
            DeathIssueIds = new List<int>();
            MovieIds = new List<int>();
            Powers = new List<String>();
            TeamIds = new List<int>();
            Teams = new ObservableCollection<Team>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
