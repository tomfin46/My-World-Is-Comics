using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Concept : ResponseSchema
    {
        public string Aliases { get; set; }
        public int Count_Of_Isssue_Appearances { get; set; }
        public Issue First_Appeared_In_Issue { get; set; }
        public ObservableCollection<Issue> Issue_Credits { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public string Start_Year { get; set; }
        public ObservableCollection<Volume> Volume_Credits { get; set; }

        public Concept()
        {
            Issue_Credits = new ObservableCollection<Issue>();
            Movies = new ObservableCollection<Movie>();
            Volume_Credits = new ObservableCollection<Volume>();
        }
    }
}
