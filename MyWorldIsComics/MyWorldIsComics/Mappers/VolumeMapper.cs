using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;

namespace MyWorldIsComics.Mappers
{
    class VolumeMapper
    {
        private Volume _volumeToMap;

        public VolumeMapper()
        {
            _volumeToMap = new Volume();
        }

        public int GetIssueCount(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return 0;
                reader.ReadToFollowing("results");
                reader.ReadToFollowing("count_of_issues");
                return reader.ReadContentAsInt();
            }
        }

        public Volume QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _volumeToMap;
                reader.ReadToFollowing("results");
                _volumeToMap = GenericResourceMapper.ParseDescriptionString(reader, _volumeToMap) as Volume;
                _volumeToMap = GenericResourceMapper.ParseId(reader, _volumeToMap) as Volume;
                _volumeToMap = GenericResourceMapper.ParseImage(reader, _volumeToMap) as Volume;
                _volumeToMap = GenericResourceMapper.ParseName(reader, _volumeToMap) as Volume;
            }
            return _volumeToMap;
        }

        public Volume MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Volume {Name = ServiceConstants.QueryNotFound};
                reader.ReadToFollowing("results");
                ParseIssueCount(reader);
                _volumeToMap = GenericResourceMapper.ParseDescriptionString(reader, _volumeToMap) as Volume;
                ParseFirstIssue(reader);
                _volumeToMap = GenericResourceMapper.ParseId(reader, _volumeToMap) as Volume;
                _volumeToMap = GenericResourceMapper.ParseImage(reader, _volumeToMap) as Volume;
                ParseIssues(reader);
                _volumeToMap = GenericResourceMapper.ParseName(reader, _volumeToMap) as Volume;
                ParsePublisher(reader);
                ParseStartYear(reader);
            }
            return _volumeToMap;
        }

        private void ParseIssueCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issues") { reader.ReadToFollowing("count_of_issues"); }
            _volumeToMap.CountOfIssues = reader.ReadElementContentAsInt();
        }

        private void ParseFirstIssue(XmlReader reader)
        {
            if (reader.Name != "first_issue") { reader.ReadToFollowing("first_issue"); }
            reader.ReadToDescendant("id");
            _volumeToMap.FirstIssueId = reader.ReadElementContentAsInt();
        }

        private void ParseIssues(XmlReader reader)
        {
            if (reader.Name != "issues") { reader.ReadToFollowing("issues"); }
            while (reader.Read())
            {
                if (reader.Name == "issue" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _volumeToMap.IssueIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "issues" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        public void ParsePublisher(XmlReader reader)
        {
            if (reader.Name != "publisher") { reader.ReadToFollowing("publisher"); }
            reader.ReadToFollowing("id");
            _volumeToMap.PublisherId = reader.ReadElementContentAsInt();
            _volumeToMap.PublisherName = reader.ReadElementContentAsString();
        }

        public void ParseStartYear(XmlReader reader)
        {
            if (reader.Name != "start_year") { reader.ReadToFollowing("start_year"); }
            _volumeToMap.StartYear = reader.ReadElementContentAsString();
        }
    }
}
