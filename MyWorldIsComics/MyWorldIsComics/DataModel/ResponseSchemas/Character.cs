using System;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.Enums;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Character : ResponseSchema
    {
        public string Birth { get; set; }
        public DateTime BirthDateTime
        {
            get
            {
                if (Birth == null) return default(DateTime);

                var commaPos = Birth.IndexOf(',');
                if (commaPos == -1)
                {
                    var dateVals = Birth.Split('-');
                    return new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2].Substring(0, 2)));
                }

                var year = int.Parse(Birth.Substring(commaPos + 1));
                var month = Month.GetMonthInt(Birth.Substring(0, 3));
                var day = int.Parse(Birth.Substring(4, commaPos - 4));

                return new DateTime(year, month, day);
            }
        }
        public string BirthFormattedString
        {
            get
            {
                return BirthDateTime == default(DateTime) ? "Unknown" : BirthDateTime.ToString("d MMM yyyy");
            }
        }

        public ObservableCollection<Character> Character_Enemies { get; set; }
        public ObservableCollection<Character> Character_Friends { get; set; }
        public int Count_Of_Issue_Appearances { get; set; }
        public ObservableCollection<Person> Creators { get; set; }
        public Issue First_Appeared_In_Issue { get; set; }
        public int Gender { get; set; }
        public ObservableCollection<Issue> Issue_Credits { get; set; }
        public ObservableCollection<Issue> Issues_Died_In { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public Origin Origin { get; set; }
        public ObservableCollection<Power> Powers { get; set; }
        public string Real_Name { get; set; }
        public string RealNameFormattedString
        {
            get
            {
                if (string.IsNullOrEmpty(Real_Name)) return "N/A";

                return Real_Name;
            }
        }
        public ObservableCollection<StoryArc> Story_Arc_Credits { get; set; }
        public ObservableCollection<Character> Team_Enemies { get; set; }
        public ObservableCollection<Character> Team_Friends { get; set; }
        public ObservableCollection<Team> Teams { get; set; }
        public ObservableCollection<Volume> Volume_Credits { get; set; }

        public Character()
        {
            Character_Enemies = new ObservableCollection<Character>();
            Character_Friends = new ObservableCollection<Character>();
            Creators = new ObservableCollection<Person>();
            Issue_Credits = new ObservableCollection<Issue>();
            Issues_Died_In = new ObservableCollection<Issue>();
            Movies = new ObservableCollection<Movie>();
            Powers = new ObservableCollection<Power>();
            Story_Arc_Credits = new ObservableCollection<StoryArc>();
        }

    }

}
