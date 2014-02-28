using System;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.Enums;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Person : ResponseSchema
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

        public int Count_Of_Issue_Appearances { get; set; }
        public string Country { get; set; }
        public ObservableCollection<Character> Created_Characters { get; set; }
        
        public string Death { get; set; }
        public DateTime DeathDateTime
        {
            get
            {
                if (Death == null) return default(DateTime);

                var commaPos = Death.IndexOf(',');
                if (commaPos == -1)
                {
                    var dateVals = Death.Split('-');
                    return new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2].Substring(0, 2)));
                }

                var year = int.Parse(Death.Substring(commaPos + 1));
                var month = Month.GetMonthInt(Death.Substring(0, 3));
                var day = int.Parse(Death.Substring(4, commaPos - 4));

                return new DateTime(year, month, day);
            }
        }
        public string DeathFormattedString
        {
            get
            {
                return DeathDateTime == default(DateTime) ? "Unknown" : DeathDateTime.ToString("d MMM yyyy");
            }
        }

        public string Email { get; set; }
        public int Gender { get; set; }
        public string Hometown { get; set; }
        public ObservableCollection<Issue> Issues { get; set; }
        public string Role { get; set; }
        public ObservableCollection<StoryArc> Story_Arc_Credits { get; set; }
        public ObservableCollection<Volume> Volume_Credits { get; set; }
        public string Website { get; set; }

        public Person()
        {
            Created_Characters = new ObservableCollection<Character>();
            Issues = new ObservableCollection<Issue>();
            Story_Arc_Credits = new ObservableCollection<StoryArc>();
            Volume_Credits = new ObservableCollection<Volume>();
        }
    }
}
