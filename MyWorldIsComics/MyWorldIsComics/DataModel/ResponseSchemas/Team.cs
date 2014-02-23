using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Team : ResponseSchema
    {
        public ObservableCollection<Character> Character_Enemies { get; set; }
        public ObservableCollection<Character> Character_Friends { get; set; }
        public ObservableCollection<Character> Characters { get; set; }
        public int Count_Of_Issue_Appearances { get; set; }
        public int Count_Of_Team_Members { get; set; }
        public ObservableCollection<Issue> Disbanded_In_Issues { get; set; }
        public Issue First_Appeared_In_Issue { get; set; }
        public ObservableCollection<Issue> Issues_Disbanded_In { get; set; }
        public ObservableCollection<Issue> Issue_Credits { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<StoryArc> Story_Arc_Credits { get; set; }
        public ObservableCollection<Volume> Volume_Credits { get; set; }
        
        public Team()
        {
            Character_Enemies = new ObservableCollection<Character>();
            Character_Friends = new ObservableCollection<Character>();
            Characters = new ObservableCollection<Character>();
            Disbanded_In_Issues = new ObservableCollection<Issue>();
            Issues_Disbanded_In = new ObservableCollection<Issue>();
            Issue_Credits = new ObservableCollection<Issue>();
            Movies = new ObservableCollection<Movie>();
            Story_Arc_Credits = new ObservableCollection<StoryArc>();
            Volume_Credits = new ObservableCollection<Volume>();
        }
    }
}
