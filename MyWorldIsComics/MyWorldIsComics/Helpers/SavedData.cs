using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Resources;

namespace MyWorldIsComics.Helpers
{
    class SavedData
    {
        public static Character BasicCharacter { get; set; }
        public static Character Character { get; set; }
        public static Team Team { get; set; }
        public static Volume Volume { get; set; }
        public static Location Location { get; set; }
        public static Section DescriptionSection { get; set; }
        public static Publisher Publisher { get; set; }
        public static Concept Concept { get; set; }
        public static Creator Creator { get; set; }
        public static Movie Movie { get; set; }
        public static StoryArc StoryArc { get; set; }
        public static ObjectResource Object { get; set; }
        public static Results TrendingCharacters { get; set; }

        public static void Clear()
        {
            Character = null;
            BasicCharacter = null;
            Team = null;
            Volume = null;
            Location = null;
            DescriptionSection = null;
            Publisher = null;
            Concept = null;
            Creator = null;
            Movie = null;
            StoryArc = null;
            Object = null;
            TrendingCharacters = null;
        }
    }
}
