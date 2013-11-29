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
                reader.ReadToFollowing("results");
                ParseId(reader);
                ParseImage(reader);
                ParseName(reader);
            }
            return _creatorToMap;
        }

        public Creator MapFilteredXmlObject(Creator basicCreator, string filteredCreatorString, string filter)
        {
            _creatorToMap = basicCreator;
            switch (filter)
            {
                case "person_credits":
                    using (XmlReader reader = XmlReader.Create(new StringReader(filteredCreatorString)))
                    {
                        reader.ReadToFollowing("results");
                        //this.ParsePeople(reader);
                    }
                    break;
            }
            return _creatorToMap;
        }

        private
            void ParseBirth(XmlReader reader)
        {
            if (reader.Name != "birth") { reader.ReadToFollowing("birth"); }
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            _creatorToMap.Birth = new DateTime(year, month, day);
        }

        private void ParseDeck(XmlReader reader)
        {
            if (reader.Name != "deck") { reader.ReadToFollowing("deck"); }
            _creatorToMap.Deck = reader.ReadElementContentAsString();
        }

        private void ParseDescriptionString(XmlReader reader)
        {
            if (reader.Name != "description") { reader.ReadToFollowing("description"); }
            _creatorToMap.DescriptionString = reader.ReadElementContentAsString();
        }

        private void ParseGender(XmlReader reader)
        {
            if (reader.Name != "gender") { reader.ReadToFollowing("gender"); }
            _creatorToMap.Gender = Gender.GetGender(reader.ReadElementContentAsInt());
        }

        private void ParseId(XmlReader reader)
        {
            if (reader.Name != "id") { reader.ReadToFollowing("id"); }
            _creatorToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private void ParseImage(XmlReader reader)
        {
            if (reader.Name != "image") { reader.ReadToFollowing("image"); }
            if (reader.IsEmptyElement) return;

            reader.ReadToFollowing("super_url");
            _creatorToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }
       
        private void ParseName(XmlReader reader)
        {
            if (reader.Name != "name") { reader.ReadToFollowing("name"); }
            _creatorToMap.Name = reader.ReadElementContentAsString();
        }
    }
}
