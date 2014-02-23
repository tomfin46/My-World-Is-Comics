using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class StoryArc : ResponseSchema
    {
        public int Count_Of_Issue_Appearances { get; set; }

        public int IssueAppearancesCount
        {
            get { return Issues.Count; }
        }

        public Issue First_Appeared_In_Issue { get; set; }
        public ObservableCollection<Issue> Issues { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        
        public StoryArc()
        {
            Issues = new ObservableCollection<Issue>();
            Movies = new ObservableCollection<Movie>();
        }
    }
}
