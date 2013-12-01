using System.IO;
using System.Xml;
using Object = MyWorldIsComics.DataModel.Resources.Object;

namespace MyWorldIsComics.Mappers
{
    class ObjectMapper
    {
        private Object _objectToMap;

        public ObjectMapper()
        {
            _objectToMap = new Object();
        }

        public Object QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                _objectToMap = GenericResourceMapper.ParseId(reader, _objectToMap) as Object;
                _objectToMap = GenericResourceMapper.ParseImage(reader, _objectToMap) as Object;
                _objectToMap = GenericResourceMapper.ParseName(reader, _objectToMap) as Object;
            }
            return _objectToMap;
        }
    }
}
