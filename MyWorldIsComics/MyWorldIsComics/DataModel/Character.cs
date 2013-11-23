using System;
using System.Collections.Generic;

namespace MyWorldIsComics.DataModel
{
    using MyWorldIsComics.DataModel.Enums;

    class Character
    {
        public int UniqueId { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public List<String> Aliases { get; set; }
        public Uri ComicVineApiUrl { get; set; }
        public Uri ComicVineSiteUrl { get; set; }
        public DateTime Birth { get; set; }
        public List<Character> Enemies { get; set; }
        public List<Character> Friends { get; set; }
        public int IssueAppearancesCount { get; set; }
        public List<Creator> Creators { get; set; }
        public string Deck { get; set; }
        public string DescriptionString { get; set; }
        public Description Description { get; set; }
        public Issue FirstAppearance { get; set; }
        public Gender.GenderEnum Gender { get; set; }
        public Uri MainImage { get; set; }
        public List<Issue> IssuesDiedIn { get; set; }
        public List<Movie> Movies { get; set; }
        public Origin Origin { get; set; }
        public List<String> Powers { get; set; }
        public Publisher Publisher { get; set; }
        public List<Team> TeamsMemberOf { get; set; }

        public Character()
        {
            Aliases = new List<string>();
            Enemies = new List<Character>();
            Friends = new List<Character>();
            Creators = new List<Creator>();
            IssuesDiedIn = new List<Issue>();
            Movies = new List<Movie>();
            Powers = new List<String>();
            TeamsMemberOf = new List<Team>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
