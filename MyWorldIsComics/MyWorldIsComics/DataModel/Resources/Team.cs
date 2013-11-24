namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

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

        #endregion

        #region Team Specific Fields

        public Issue FirstAppearance { get; set; }
        public int IssueAppearancesCount { get; set; }
        public int MembersCount { get; set; }
        public Publisher Publisher { get; set; }

        #region Collections

        public List<String> Aliases { get; set; }
        public List<Character> Members { get; set; }
        public List<Character> Enemies { get; set; }
        public List<Character> Friends { get; set; }
        public List<Issue> IssuesDispandedIn { get; set; }
        public List<Movie> Movies { get; set; }

        #endregion

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
