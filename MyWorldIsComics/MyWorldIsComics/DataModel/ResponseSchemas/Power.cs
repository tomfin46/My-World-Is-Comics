using System.Collections.ObjectModel;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Power : ResponseSchema
    {
        public ObservableCollection<Character> Characters { get; set; }

        public Power()
        {
            Characters = new ObservableCollection<Character>();
        }
    }
}
