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
    }
}
