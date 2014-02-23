using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Power : ResponseSchema
    {
        public string Aliases { get; set; }
        public ObservableCollection<Character> Characters { get; set; }

        public Power()
        {
            Characters = new ObservableCollection<Character>();
        }
    }
}
