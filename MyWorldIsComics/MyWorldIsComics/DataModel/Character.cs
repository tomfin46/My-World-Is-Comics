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
        public Uri ApiUrl { get; set; }
        public DateTime Birth { get; set; }
        public List<Character> Enemies { get; set; }
        public List<Character> Friends { get; set; }
        public int IssueAppearancesCount { get; set; }
        public List<Creator> Creators { get; set; }
        public string Deck { get; set; }
        public string Description { get; set; }
        public Issue FirstAppearance { get; set; }
        public Gender Gender { get; set; }
        public Uri MainImage { get; set; }
        public List<Issue> AllAppearances { get; set; }
        public List<Issue> IssuesDiedIn { get; set; }
        public List<Movie> Movies { get; set; }
        public string Origin { get; set; }
        public List<Power> Powers { get; set; }
        public Publisher Publisher { get; set; }
        public List<StoryArc> StoryArcs { get; set; }
        public List<Team> EnemyTeams { get; set; }
        public List<Team> FriendlyTeams { get; set; }
        public List<Team> TeamsMemberOf { get; set; }
        public List<ComicSeriesVolume> VolumeCredits { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
