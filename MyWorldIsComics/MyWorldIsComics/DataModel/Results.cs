using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel
{
    class Results
    {
        public string Name { get; set; }
        public ObservableCollection<IResponse> ResultsList { get; set; }
    }
}
