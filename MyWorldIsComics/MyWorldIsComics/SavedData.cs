using System.Collections.Generic;

namespace MyWorldIsComics
{
    using MyWorldIsComics.DataModel.Resources;

    class SavedData
    {
        public static Character QuickCharacter { get; set; }
        public static Character Character { get; set; }
        public static List<Team> QuickTeams { get; set; }
        public static List<Team> Teams { get; set; }
    }
}
