﻿using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Location : ResponseSchema
    {
        public int Count_Of_Issue_Appearances { get; set; }
        public Issue First_Appeared_In_Issue { get; set; }
        public ObservableCollection<Issue> Issue_Credits { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public string Start_Year { get; set; }
        public ObservableCollection<StoryArc> Story_Arc_Credits { get; set; }
        public ObservableCollection<Volume> Volume_Credits { get; set; }
        
        public Location()
        {
            Issue_Credits = new ObservableCollection<Issue>();
            Movies = new ObservableCollection<Movie>();
            Story_Arc_Credits = new ObservableCollection<StoryArc>();
            Volume_Credits = new ObservableCollection<Volume>();
        }

        
    }
}
