using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.Mappers
{
    using System.IO;
    using System.Xml;

    using MyWorldIsComics.DataModel.Enums;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;

    class PublisherMapper
    {
        private Publisher _publisherToMap;

        public PublisherMapper()
        {
            _publisherToMap = new Publisher();
        }

        public Publisher MapXmlObject(string characterSearchString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(characterSearchString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Publisher { Name = ServiceConstants.QueryNotFound };

                reader.ReadToFollowing("results");
                ParseAliases(reader);
                this._publisherToMap = GenericResourceMapper.ParseDeck(reader, this._publisherToMap) as Publisher;
                this._publisherToMap = GenericResourceMapper.ParseDescriptionString(reader, this._publisherToMap) as Publisher;
                this._publisherToMap = GenericResourceMapper.ParseId(reader, this._publisherToMap) as Publisher;
                this._publisherToMap = GenericResourceMapper.ParseImage(reader, this._publisherToMap) as Publisher;
                ParseLocation(reader);
                this._publisherToMap = GenericResourceMapper.ParseName(reader, this._publisherToMap) as Publisher;
            }
            return this._publisherToMap;
        }

        private void ParseAliases(XmlReader reader)
        {
            _publisherToMap.Aliases.Clear();
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n');
            _publisherToMap.Aliases.AddRange(aliases);
        }

        private void ParseLocation(XmlReader reader)
        {
            if (reader.Name != "location_address") { reader.ReadToFollowing("location_address"); }
            _publisherToMap.Location.Address = reader.ReadElementContentAsString();
            if (reader.Name != "location_city") { reader.ReadToFollowing("location_city"); }
            _publisherToMap.Location.City = reader.ReadElementContentAsString();
            if (reader.Name != "location_state") { reader.ReadToFollowing("location_state"); }
            _publisherToMap.Location.State = reader.ReadElementContentAsString();
        }
    }
}
