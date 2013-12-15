using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;

namespace MyWorldIsComics.Mappers
{
    class CreatorMapper
    {
         private Creator _creatorToMap;

        public CreatorMapper()
        {
            _creatorToMap = new Creator();
        }

        public Creator QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _creatorToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _creatorToMap = GenericResourceMapper.ParseId(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseImage(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseName(reader, _creatorToMap) as Creator;
            }
            return _creatorToMap;
        }

        public Creator MapXmlObject(string creatorString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(creatorString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Creator { Name = ServiceConstants.QueryNotFound };

                reader.ReadToFollowing("results");
                ParseBirth(reader);
                ParseCountry(reader);
                ParseCreatedCharacters(reader);
                ParseDeath(reader);
                _creatorToMap = GenericResourceMapper.ParseDeck(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseDescriptionString(reader, _creatorToMap) as Creator;
                ParseHometown(reader);
                _creatorToMap = GenericResourceMapper.ParseId(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseImage(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseName(reader, _creatorToMap) as Creator;
                ParseWebsite(reader);
            }
            return _creatorToMap;
        }

        private void ParseBirth(XmlReader reader)
        {
            if (reader.Name != "birth") { reader.ReadToFollowing("birth"); }
            if (reader.IsEmptyElement) return;
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            _creatorToMap.Birth = new DateTime(year, month, day);
        }

        private void ParseCountry(XmlReader reader)
        {
            if (reader.Name != "country") { reader.ReadToFollowing("country"); }
            if (reader.IsEmptyElement) return;
            _creatorToMap.Country = reader.ReadElementContentAsString();
        }

        private void ParseCreatedCharacters(XmlReader reader)
        {
            if (reader.Name != "characters") { reader.ReadToFollowing("characters"); }
            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _creatorToMap.CreatedCharacterIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "characters" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseDeath(XmlReader reader)
        {
            if (reader.Name != "death") { reader.ReadToFollowing("death"); }
            if (reader.IsEmptyElement) return;
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            _creatorToMap.Death = new DateTime(year, month, day);
        }

        private void ParseHometown(XmlReader reader)
        {
            if (reader.Name != "hometown") { reader.ReadToFollowing("hometown"); }
            if (reader.IsEmptyElement) return;
            _creatorToMap.Hometown = reader.ReadElementContentAsString();
        }

        private void ParseWebsite(XmlReader reader)
        {
            if (reader.Name != "website") { reader.ReadToFollowing("website"); }
            if (reader.IsEmptyElement) return;
            _creatorToMap.Website = new Uri(reader.ReadElementContentAsString());
        }
    }
}
