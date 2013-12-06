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
    }
}
