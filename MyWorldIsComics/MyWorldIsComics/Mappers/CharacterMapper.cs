using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.Mappers
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using Windows.Data.Json;

    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.Enums;

    using Newtonsoft.Json;

    class CharacterMapper
    {
        private static Character characterToMap = new Character();

        public static void MapJsonObject(string jsonString, out Character character)
        {
            var serializer = new XmlSerializer(typeof(Character));
            //character = serializer.Deserialize(jsonString);
            character = JsonConvert.DeserializeObject<Character>(jsonString);
            var obj = JsonConvert.DeserializeObjectAsync(jsonString);
        }

        public static void MapXmlObject(string xmlString, out Character character)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("character");
                ParseAliases(reader);
                ParseApiUrl(reader);
                ParseBirth(reader);
                ParseAppearances(reader);
                ParseDeck(reader);
                ParseDescription(reader);
                ParseFirstAppearance(reader);
                ParseGender(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseName(reader);
                ParseOrigin(reader);
                ParsePublisher(reader);
                ParseRealName(reader);
                ParseSiteUrl(reader);
            }
            character = characterToMap;
        }

        private static void ParseAliases(XmlReader reader)
        {
            reader.ReadToFollowing("aliases");
            var aliases = reader.ReadElementContentAsString().Split('\n');
            characterToMap.Aliases.AddRange(aliases);
        }
        private static void ParseApiUrl(XmlReader reader)
        {
            characterToMap.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseBirth(XmlReader reader)
        {
            var date = reader.ReadElementContentAsString();

            if (date == "") { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            characterToMap.Birth = new DateTime(year, month, day);
        }

        private static void ParseAppearances(XmlReader reader)
        {
            characterToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private static void ParseDeck(XmlReader reader)
        {
            reader.ReadToFollowing("deck");
            characterToMap.Deck = reader.ReadElementContentAsString();
        }

        private static void ParseDescription(XmlReader reader)
        {
            characterToMap.Description = reader.ReadElementContentAsString();
        }

        private static void ParseFirstAppearance(XmlReader reader)
        {
            var issue = new Issue();

            reader.ReadToFollowing("api_detail_url");
            issue.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());

            //reader.ReadToFollowing("id");
            issue.UniqueId = reader.ReadElementContentAsInt();

            //reader.ReadToFollowing("name");
            issue.Name = reader.ReadElementContentAsString();

            //reader.ReadToFollowing("issue_number");
            issue.IssueNumber = reader.ReadElementContentAsInt();

            characterToMap.FirstAppearance = issue;
        }

        private static void ParseGender(XmlReader reader)
        {
            reader.ReadToFollowing("gender");
            characterToMap.Gender = Gender.GetGender(reader.ReadElementContentAsInt());
        }

        private static void ParseId(XmlReader reader)
        {
            //reader.ReadToFollowing("id");
            characterToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private static void ParseImage(XmlReader reader)
        {
            //reader.ReadToFollowing("image");
            reader.ReadToFollowing("super_url");
            characterToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }
        private static void ParseName(XmlReader reader)
        {
            reader.ReadToFollowing("name");
            characterToMap.Name = reader.ReadElementContentAsString();
        }

        private static void ParseOrigin(XmlReader reader)
        {
            //reader.ReadToFollowing("origin");
            var origin = new Origin();

            reader.ReadToFollowing("api_detail_url");
            origin.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());

            //reader.ReadToFollowing("id");
            origin.UniqueId = reader.ReadElementContentAsInt();

            //reader.ReadToFollowing("name");
            origin.Name = reader.ReadElementContentAsString();

            characterToMap.Origin = origin;
        }

        private static void ParsePublisher(XmlReader reader)
        {
            //reader.ReadToFollowing("publisher");
            var publisher = new Publisher();

            reader.ReadToFollowing("api_detail_url");
            publisher.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());

            //reader.ReadToFollowing("id");
            publisher.UniqueId = reader.ReadElementContentAsInt();

            //reader.ReadToFollowing("name");
            publisher.Name = reader.ReadElementContentAsString();

            characterToMap.Publisher = publisher;
        }

        private static void ParseRealName(XmlReader reader)
        {
            reader.ReadToFollowing("real_name");
            characterToMap.RealName = reader.ReadElementContentAsString();
        }

        private static void ParseSiteUrl(XmlReader reader)
        {
            //reader.ReadToFollowing("site_detail_url");
            characterToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());
        }
    }
}
