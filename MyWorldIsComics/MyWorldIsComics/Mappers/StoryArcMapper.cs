using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel.Resources;
using Object = System.Object;

namespace MyWorldIsComics.Mappers
{
    class StoryArcMapper
    {
        private StoryArc _storyArcToMap;

        public StoryArcMapper()
        {
            _storyArcToMap = new StoryArc();
        }

        public StoryArc QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _storyArcToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _storyArcToMap = GenericResourceMapper.ParseId(reader, _storyArcToMap) as StoryArc;
                _storyArcToMap = GenericResourceMapper.ParseImage(reader, _storyArcToMap) as StoryArc;
                _storyArcToMap = GenericResourceMapper.ParseName(reader, _storyArcToMap) as StoryArc;
            }
            return _storyArcToMap;
        }
    }
}
