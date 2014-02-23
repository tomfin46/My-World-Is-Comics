using System;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Volume : ResponseSchema
    {
        public ObservableCollection<Character> Characters { get; set; }
        public ObservableCollection<Concept> Concepts { get; set; }
        public int Count_Of_Issues { get; set; }
        public Issue First_Issue { get; set; }
        public ObservableCollection<Issue> Issues { get; set; }
        public Issue Last_Issue { get; set; }
        public ObservableCollection<Location> Locations { get; set; }
        public ObservableCollection<ObjectResource> Objects { get; set; }
        public ObservableCollection<Person> People { get; set; }
        public string Start_Year { get; set; }

        public Section DescriptionSection { get; set; }

        public string DescriptionSectionString
        {
            get
            {
                return DescriptionSection == null ? String.Empty : DescriptionSection.ToPlainString();
            }
        }

        public Volume()
        {
            Characters = new ObservableCollection<Character>();
            Concepts = new ObservableCollection<Concept>();
            Issues = new ObservableCollection<Issue>();
            Locations = new ObservableCollection<Location>();
            Objects = new ObservableCollection<ObjectResource>();
            People = new ObservableCollection<Person>();
        }
    }
}
