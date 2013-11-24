﻿namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using Interfaces;

    public class Movie : IResource
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

        public override string ToString()
        {
            return Name;
        }
    }
}