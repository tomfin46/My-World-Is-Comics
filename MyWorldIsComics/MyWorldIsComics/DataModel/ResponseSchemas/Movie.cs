using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Movie : ResponseSchema
    {
        public string Box_Office_Revenue { get; set; }
        public string BoxOfficeRevenueFormattedString
        {
            get
            {
                return Box_Office_Revenue == String.Empty ? "N/A" : int.Parse(Box_Office_Revenue).ToString("C0", new CultureInfo("en-US"));
            }
        }

        public string Budget { get; set; }
        public string BudgetFormattedString
        {
            get
            {
                return Budget == String.Empty ? "N/A" : int.Parse(Budget).ToString("C0", new CultureInfo("en-US"));
            }
        }

        public ObservableCollection<Character> Characters { get; set; }
        public ObservableCollection<Concept> Concepts { get; set; }
        public bool Has_Staff_Review { get; set; }
        public ObservableCollection<Location> Locations { get; set; }
        public ObservableCollection<Person> Producers { get; set; }
        public string Rating { get; set; }
        public string Release_Date { get; set; }
        public string Runtime { get; set; }
        public ObservableCollection<Team> Teams { get; set; }
        public ObservableCollection<ObjectResource> Things { get; set; }
        public string Total_Revenue { get; set; }
        public string TotalRevenueFormattedString
        {
            get
            {
                return Total_Revenue == String.Empty ? "N/A" : int.Parse(Total_Revenue).ToString("C0", new CultureInfo("en-US"));
            }
        }

        public ObservableCollection<Person> Writers { get; set; }

        public Movie()
        {
            Characters = new ObservableCollection<Character>();
            Concepts = new ObservableCollection<Concept>();
            Locations = new ObservableCollection<Location>();
            Producers = new ObservableCollection<Person>();
            Teams = new ObservableCollection<Team>();
            Things = new ObservableCollection<ObjectResource>();
            Writers = new ObservableCollection<Person>();
        }
    }
}
