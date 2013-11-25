using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyWorldIsComics
{
    using MyWorldIsComics.DataModel.Resources;

    class SavedData
    {
        public static Character QuickCharacter { get; set; }
        public static Character Character { get; set; }
        public static ObservableCollection<Team> QuickTeams { get; set; }
        public static ObservableCollection<Team> Teams { get; set; }

        public static void Clear()
        {
            Character = null;
            QuickCharacter = null;
            Teams = null;
            QuickTeams = null;
        }
    }
}
