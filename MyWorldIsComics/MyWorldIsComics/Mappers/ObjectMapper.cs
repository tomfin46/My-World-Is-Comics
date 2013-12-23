using System.IO;
using System.Xml;
using MyWorldIsComics.DataModel.Resources;

namespace MyWorldIsComics.Mappers
{
    class ObjectMapper
    {
        private ObjectResource _objectToMap;

        public ObjectMapper()
        {
            _objectToMap = new ObjectResource();
        }

        public ObjectResource QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _objectToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _objectToMap = GenericResourceMapper.ParseId(reader, _objectToMap) as ObjectResource;
                _objectToMap = GenericResourceMapper.ParseImage(reader, _objectToMap) as ObjectResource;
                _objectToMap = GenericResourceMapper.ParseName(reader, _objectToMap) as ObjectResource;
            }
            return _objectToMap;
        }

        public ObjectResource MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _objectToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                this.ParseAliases(reader);
                this.ParseAppearanceCount(reader);
                _objectToMap = GenericResourceMapper.ParseDeck(reader, _objectToMap) as ObjectResource;
                _objectToMap = GenericResourceMapper.ParseDescriptionString(reader, _objectToMap) as ObjectResource;
                this.ParseFirstAppearance(reader);
                _objectToMap = GenericResourceMapper.ParseId(reader, _objectToMap) as ObjectResource;
                _objectToMap = GenericResourceMapper.ParseImage(reader, _objectToMap) as ObjectResource;
                _objectToMap = GenericResourceMapper.ParseName(reader, _objectToMap) as ObjectResource;
            }
            return _objectToMap;
        }

        private void ParseAliases(XmlReader reader)
        {
            _objectToMap.Aliases.Clear();
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n');
            _objectToMap.Aliases.AddRange(aliases);
        }

        private void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issue_appearances") { reader.ReadToFollowing("count_of_issue_appearances"); }
            _objectToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }
        
        private void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            if (reader.IsEmptyElement) return;
            reader.ReadToDescendant("id");
            _objectToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }
    }
}
