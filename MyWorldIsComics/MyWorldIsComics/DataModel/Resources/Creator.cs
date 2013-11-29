using System;
using System.Collections.Generic;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.Resources
{
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

        #endregion

        #region IPerson Fields
        public DateTime Birth { get; set; }
        public Gender.GenderEnum Gender { get; set; }

        #endregion

        public string Role { get; set; }


        public override string ToString()
        {
            return Name;
        }
    }
}
