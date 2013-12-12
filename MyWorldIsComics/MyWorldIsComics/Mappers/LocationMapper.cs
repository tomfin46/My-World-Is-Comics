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

    class LocationMapper
    {
        private Location _locationToMap;

        public LocationMapper()
        {
            _locationToMap = new Location();
        }

        public Location QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _locationToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _locationToMap = GenericResourceMapper.ParseId(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseImage(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseName(reader, _locationToMap) as Location;
            }
            return _locationToMap;
        }

        public Location MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Location { Name = ServiceConstants.QueryNotFound };
                if (reader.Name == "response" || reader.Name != "results") reader.ReadToFollowing("results");

                ParseAliases(reader);
                this.ParseAppearanceCount(reader);
                _locationToMap = GenericResourceMapper.ParseDeck(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseDescriptionString(reader, _locationToMap) as Location;
                ParseFirstAppearance(reader);
                _locationToMap = GenericResourceMapper.ParseId(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseImage(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseName(reader, _locationToMap) as Location;
                ParseVolumeAppearances(reader);
            }
            return _locationToMap;
        }

        private void ParseAliases(XmlReader reader)
        {
            _locationToMap.Aliases.Clear();
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n');
            _locationToMap.Aliases.AddRange(aliases);
        }

        private void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issue_appearances") { reader.ReadToFollowing("count_of_issue_appearances"); }
            _locationToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            reader.ReadToDescendant("id");
            _locationToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }

        private void ParseVolumeAppearances(XmlReader reader)
        {
            if (reader.Name != "volume_credits") { reader.ReadToFollowing("volume_credits"); }
            while (reader.Read())
            {
                if (reader.Name == "volume" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _locationToMap.VolumeIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "volume_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }
    }
}
