using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel
{
    class Results
    {
        public string Name { get; set; }
        public ObservableCollection<IResource> ResultsList { get; set; }
    }
}
