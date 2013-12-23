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

        public StoryArc MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _storyArcToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _storyArcToMap = GenericResourceMapper.ParseDeck(reader, _storyArcToMap) as StoryArc;
                _storyArcToMap = GenericResourceMapper.ParseDescriptionString(reader, _storyArcToMap) as StoryArc;
                this.ParseFirstAppearance(reader);
                _storyArcToMap = GenericResourceMapper.ParseId(reader, _storyArcToMap) as StoryArc;
                _storyArcToMap = GenericResourceMapper.ParseImage(reader, _storyArcToMap) as StoryArc;
                this.ParseIssues(reader);
                _storyArcToMap = GenericResourceMapper.ParseName(reader, _storyArcToMap) as StoryArc;
            }
            return _storyArcToMap;
        }

        private void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            if (reader.IsEmptyElement) return;
            reader.ReadToDescendant("id");
            _storyArcToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }

        private void ParseIssues(XmlReader reader)
        {
            if (reader.Name != "issues") { reader.ReadToFollowing("issues"); }
            while (reader.Read())
            {
                if (reader.Name == "issue" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _storyArcToMap.IssueIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "issues" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }
    }
}
