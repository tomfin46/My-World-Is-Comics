using System.Collections.Generic;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.DataModel
{
    class Description
    {
        public Section CurrentEvents { get; set; }
        public Section Origin { get; set; }
        public Section Creation { get; set; }
        public Section CharacterEvolution { get; set; }
        public Section MajorStoryArcs { get; set; }
        public Section AlternateRealities { get; set; }
    }
}
