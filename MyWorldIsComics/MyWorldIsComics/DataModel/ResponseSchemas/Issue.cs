using System;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Issue : ResponseSchema
    {
        #region Json Response Fields

        public string Aliases { get; set; }
        public ObservableCollection<Character> Character_Credits { get; set; }
        public ObservableCollection<Character> Character_Died_In { get; set; }
        public ObservableCollection<Concept> Concept_Credits { get; set; }

        public string Cover_Date { get; set; }
        public DateTime CoverDateDateTime
        {
            get
            {
                if (Cover_Date == null) return default(DateTime);

                var dateVals = Cover_Date.Split('-');
                return new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2]));
            }
        }
        public string CoverDateFormattedString
        {
            get
            {
                return CoverDateDateTime == default(DateTime) ? "Unknown" : CoverDateDateTime.ToString("d MMM yyyy");
            }
        }

        public ObservableCollection<Character> First_Appearance_Characters { get; set; }
        public ObservableCollection<Concept> First_Appearance_Concepts { get; set; }
        public ObservableCollection<Location> First_Appearance_Locations { get; set; }
        public ObservableCollection<ObjectResource> First_Appearance_Objects { get; set; }
        public ObservableCollection<StoryArc> First_Appearance_StoryArcs { get; set; }
        public ObservableCollection<Team> First_Appearance_Teams { get; set; }
        public bool Has_Staff_Review { get; set; }

        public string Issue_Number { get; set; }
        public string IssueNumberFormattedString
        {
            get
            {
                return Issue_Number != null ? "#" + Issue_Number : String.Empty;
            }
        }

        public int IssueNumberInteger
        {
            get
            {
                int issueNumber;
                try
                {
                    issueNumber = int.Parse(Issue_Number);
                }
                catch (FormatException)
                {
                    issueNumber = Convert.ToInt32(Issue_Number.Substring(0, Issue_Number.IndexOf('.')));
                }
                return issueNumber;
            }
        }

        public ObservableCollection<Location> Location_Credits { get; set; }
        public ObservableCollection<ObjectResource> Object_Credits { get; set; }
        public ObservableCollection<Person> Person_Credits { get; set; }

        public ObservableCollection<Person> PersonCredits
        {
            get { return Person_Credits; }
        }

        public string Store_Date { get; set; }
        public DateTime StoreDateDateTime
        {
            get
            {
                if (Store_Date == null) return default(DateTime);

                var dateVals = Store_Date.Split('-');
                return new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2]));
            }
        }
        public string StoreDateFormattedString
        {
            get
            {
                return StoreDateDateTime == default(DateTime) ? "Unknown" : StoreDateDateTime.ToString("d MMM yyyy");
            }
        }

        public ObservableCollection<StoryArc> Story_Arc_Credits { get; set; }
        public ObservableCollection<Team> Team_Credits { get; set; }
        public ObservableCollection<Team> Team_Disbanded_In { get; set; }
        public Volume Volume { get; set; }

        #endregion

        public Section DescriptionSection { get; set; }

        public string DescriptionSectionString
        {
            get
            {
                return DescriptionSection == null ? String.Empty : DescriptionSection.ToPlainString();
            }
        }

        public Issue()
        {
            Character_Credits = new ObservableCollection<Character>();
            Character_Died_In = new ObservableCollection<Character>();
            Concept_Credits = new ObservableCollection<Concept>();
            First_Appearance_Characters = new ObservableCollection<Character>();
            First_Appearance_Concepts = new ObservableCollection<Concept>();
            First_Appearance_Locations = new ObservableCollection<Location>();
            First_Appearance_Objects = new ObservableCollection<ObjectResource>();
            First_Appearance_StoryArcs = new ObservableCollection<StoryArc>();
            First_Appearance_Teams = new ObservableCollection<Team>();
            Location_Credits = new ObservableCollection<Location>();
            Object_Credits = new ObservableCollection<ObjectResource>();
            Person_Credits = new ObservableCollection<Person>();
            Story_Arc_Credits = new ObservableCollection<StoryArc>();
            Team_Credits = new ObservableCollection<Team>();
            Team_Disbanded_In = new ObservableCollection<Team>();
        }
    }
}
