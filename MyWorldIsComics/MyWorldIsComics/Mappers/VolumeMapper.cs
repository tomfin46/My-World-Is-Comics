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
    }
}
