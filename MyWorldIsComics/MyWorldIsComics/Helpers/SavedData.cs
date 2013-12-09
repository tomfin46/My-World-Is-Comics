using MyWorldIsComics.DataModel.Resources;

namespace MyWorldIsComics.Helpers
{
    class SavedData
    {
        public static Character BasicCharacter { get; set; }
        public static Character Character { get; set; }
        public static Volume Volume { get; set; }

        public static void Clear()
        {
            Character = null;
            BasicCharacter = null;
            Volume = null;
        }
    }
}
