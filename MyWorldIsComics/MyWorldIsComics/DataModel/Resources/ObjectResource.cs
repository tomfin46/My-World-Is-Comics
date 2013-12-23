﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
    public class ObjectResource : IResource
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
        public int FirstAppearanceId { get; set; }
        public Issue FirstAppearanceIssue { get; set; }
        public List<String> Aliases { get; set; }
        public string AliasesString
        {
            get
            {
                return Aliases.Count == 1 && Aliases.First() == String.Empty ? "None" : string.Join(", ", Aliases);
            }
        }
        public int IssueAppearancesCount { get; set; }

        public ObjectResource()
        {
            Aliases = new List<string>();
        }
    }
}
