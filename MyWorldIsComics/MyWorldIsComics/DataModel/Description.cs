using System.Collections.Generic;
using MyWorldIsComics.DataModel.DescriptionElements;

namespace MyWorldIsComics.DataModel
{
    class Description
    {
        public Queue<Paragraph> CurrentEvents { get; set; }
        public string Origin { get; set; }
        public string Creation { get; set; }
        public string CharacterEvolution { get; set; }
        public string MajorStoryArcs { get; set; }
        public string AlternateRealities { get; set; }
    }
}
