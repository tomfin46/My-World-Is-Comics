using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel.Resources;

namespace MyWorldIsComics.Mappers
{
    using MyWorldIsComics.DataSource;

    class ConceptMapper
    {
        private Concept _conceptToMap;

        public ConceptMapper()
        {
            _conceptToMap = new Concept();
        }

        public Concept QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _conceptToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _conceptToMap = GenericResourceMapper.ParseId(reader, _conceptToMap) as Concept;
                _conceptToMap = GenericResourceMapper.ParseImage(reader, _conceptToMap) as Concept;
                _conceptToMap = GenericResourceMapper.ParseName(reader, _conceptToMap) as Concept;
            }
            return _conceptToMap;
        }

        public Concept MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Concept { Name = ServiceConstants.QueryNotFound };

                reader.ReadToFollowing("results");
                ParseAppearanceCount(reader);
                _conceptToMap = GenericResourceMapper.ParseDeck(reader, _conceptToMap) as Concept;
                _conceptToMap = GenericResourceMapper.ParseDescriptionString(reader, _conceptToMap) as Concept;
                ParseFirstAppearance(reader);
                _conceptToMap = GenericResourceMapper.ParseId(reader, _conceptToMap) as Concept;
                _conceptToMap = GenericResourceMapper.ParseImage(reader, _conceptToMap) as Concept;
                _conceptToMap = GenericResourceMapper.ParseName(reader, _conceptToMap) as Concept;
            }
            return _conceptToMap;
        }

        private void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_isssue_appearances") { reader.ReadToFollowing("count_of_isssue_appearances"); }
            _conceptToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            if(reader.IsEmptyElement) return;
            reader.ReadToDescendant("id");
            _conceptToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }
    }
}
