using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyWorldIsComics
{
    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Resources;

    class SavedData
    {
        public static Character BasicCharacter { get; set; }
        public static Character Character { get; set; }

        public static void Clear()
        {
            Character = null;
            BasicCharacter = null;
        }
    }
}
