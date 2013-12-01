using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel.Enums;
using MyWorldIsComics.DataModel.Resources;

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
                reader.ReadToFollowing("results");
                _creatorToMap = GenericResourceMapper.ParseId(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseImage(reader, _creatorToMap) as Creator;
                _creatorToMap = GenericResourceMapper.ParseName(reader, _creatorToMap) as Creator;
            }
            return _creatorToMap;
        }
    }
}
