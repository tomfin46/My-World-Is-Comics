using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
    using System.Collections.ObjectModel;

    public class Creator : IResource, IPerson
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

        #region IPerson Fields
        public DateTime Birth { get; set; }
        public string BirthString
        {
            get
            {
                return Birth == default(DateTime) ? "Unknown" : Birth.ToString("d MMM yyyy");
            }
        }
        public DateTime Death { get; set; }
        public string DeathString
        {
            get
            {
                return Death == default(DateTime) ? "N/A" : Death.ToString("d MMM yyyy");
            }
        }
        public Gender.GenderEnum Gender { get; set; }

        #endregion

        public string Role { get; set; }
        public List<int> CreatedCharacterIds { get; set; }
        public ObservableCollection<Character> CreatedCharacters { get; set; }
        public string Country { get; set; }
        public string Hometown { get; set; }
        public Uri Website { get; set; }

        public Creator()
        {
            CreatedCharacterIds = new List<int>();
            CreatedCharacters = new ObservableCollection<Character>();
        }

        public override string ToString()
        {
            return Name;
        }


    }
}
