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
                reader.ReadToFollowing("results");
                _locationToMap = GenericResourceMapper.ParseId(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseImage(reader, _locationToMap) as Location;
                _locationToMap = GenericResourceMapper.ParseName(reader, _locationToMap) as Location;
            }
            return _locationToMap;
        }
    }
}
