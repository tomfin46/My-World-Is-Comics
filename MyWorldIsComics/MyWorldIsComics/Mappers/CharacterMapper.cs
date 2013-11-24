using System.Xml.Serialization;

namespace MyWorldIsComics.Mappers
{
    #region Usings

    using System.IO;
    using System.Xml;
    using System;
    using System.Collections.Generic;

    using DataModel.Resources;
    using DataModel;
    using DataModel.Enums;

    #endregion

    class CharacterMapper
    {
        private static readonly Character CharacterToMap = new Character();

        public static void QuickMapXmlObject(string xmlString, out Character character)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseAliases(reader);
                reader.ReadToFollowing("birth");
                ParseBirth(reader);
                ParseAppearanceCount(reader);
                ParseDeck(reader);
                ParseDescriptionString(reader);
                ParseFirstAppearance(reader);
                ParseGender(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseName(reader);
                ParsePublisher(reader);
                ParseRealName(reader);
            }
            character = CharacterToMap;
        }

        public static void MapXmlObject(string xmlString, out Character character)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                //ParseAliases(reader);
                //ParseComicVineApiUrl(reader);
                //ParseBirth(reader);
                //ParseEnemies(reader);
                //ParseFriends(reader);
                //ParseAppearanceCount(reader);
                //ParseCreators(reader);
                //ParseDeck(reader);
                //ParseDescriptionString(reader);
                //ParseFirstAppearance(reader);
                //ParseGender(reader);
                //ParseId(reader);
                //ParseImage(reader);
                //ParseIssuesDiedIn(reader);
                //ParseMovies(reader);
                //ParseName(reader);
                //ParseOrigin(reader);
                //ParsePowers(reader);
                //ParsePublisher(reader);
                //ParseRealName(reader);
                //ParseComicVineSiteUrl(reader);
                ParseTeamsMemberOf(reader);
            }
            character = CharacterToMap;
        }

        public static void GetCharacterSearchId(string xmlString, out int id)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("gender");
                reader.ReadToFollowing("id");
                id = reader.ReadElementContentAsInt();
            }
        }

        private static void ParseAliases(XmlReader reader)
        {
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n');
            CharacterToMap.Aliases.AddRange(aliases);
        }

        private static void ParseComicVineApiUrl(XmlReader reader)
        {
            if (reader.Name != "api_detail_url") { reader.ReadToFollowing("api_detail_url"); }
            CharacterToMap.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseBirth(XmlReader reader)
        {
            if (reader.Name != "birth") { reader.ReadToFollowing("birth"); }
            var date = reader.ReadElementContentAsString();

            if (date == "") { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            CharacterToMap.Birth = new DateTime(year, month, day);
        }

        private static void ParseEnemies(XmlReader reader)
        {
            if (reader.Name != "character_enemies") { reader.ReadToFollowing("character_enemies"); }
            reader.ReadToDescendant("character");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                CharacterToMap.EnemyIds.Add(reader.ReadElementContentAsInt());
                
                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "character") { i = 10; }
            }
        }

        private static void ParseFriends(XmlReader reader)
        {
            if (reader.Name != "character_friends") { reader.ReadToFollowing("character_friends"); }
            reader.ReadToDescendant("character");
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                CharacterToMap.FriendIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "character") { i = 10; }
            }
        }

        private static void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issue_appearances") { reader.ReadToFollowing("count_of_issue_appearances"); }

            CharacterToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private static void ParseCreators(XmlReader reader)
        {
            if (reader.Name != "creators") { reader.ReadToFollowing("creators"); }
            reader.Read();
            while (reader.Name == "creator")
            {
                reader.ReadToDescendant("id");
                CharacterToMap.CreatorIds.Add(reader.ReadElementContentAsInt());
                
                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
            }
        }

        private static void ParseDeck(XmlReader reader)
        {
            if (reader.Name != "deck") { reader.ReadToFollowing("deck"); }
            CharacterToMap.Deck = reader.ReadElementContentAsString();
        }

        private static void ParseDescriptionString(XmlReader reader)
        {
            if (reader.Name != "description") { reader.ReadToFollowing("description"); }
            CharacterToMap.DescriptionString = reader.ReadElementContentAsString();
        }

        private static void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            reader.ReadToDescendant("id");
            CharacterToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }

        private static void ParseGender(XmlReader reader)
        {
            if (reader.Name != "gender") { reader.ReadToFollowing("gender"); }
            CharacterToMap.Gender = Gender.GetGender(reader.ReadElementContentAsInt());
        }

        private static void ParseId(XmlReader reader)
        {
            if (reader.Name != "id") { reader.ReadToFollowing("id"); }
            CharacterToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private static void ParseImage(XmlReader reader)
        {
            if (reader.Name != "super_url") { reader.ReadToFollowing("super_url"); }
            CharacterToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseIssuesDiedIn(XmlReader reader)
        {
            if (reader.Name != "issues_died_in") { reader.ReadToFollowing("issues_died_in"); }
            reader.Read();
            while (reader.Name == "issue")
            {
                reader.ReadToDescendant("id");
                CharacterToMap.DeathIssueIds.Add(reader.ReadElementContentAsInt());
                
                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
            }
        }

        private static void ParseMovies(XmlReader reader)
        {
            if (reader.Name != "movies") { reader.ReadToFollowing("movies"); }
            reader.ReadToDescendant("movie");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                CharacterToMap.MovieIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "movie") { i = 10; }
            }
        }

        private static void ParseName(XmlReader reader)
        {
            if (reader.Name != "name") { reader.ReadToFollowing("name"); }
            CharacterToMap.Name = reader.ReadElementContentAsString();
        }

        private static void ParseOrigin(XmlReader reader)
        {
            if (reader.Name != "origin") { reader.ReadToFollowing("origin"); }
            reader.ReadToFollowing("name");
            CharacterToMap.Origin = reader.ReadElementContentAsString();
        }

        private static void ParsePowers(XmlReader reader)
        {
            if (reader.Name != "powers") { reader.ReadToFollowing("powers"); }
            reader.Read();

            while (reader.Name == "power")
            {
                reader.ReadToFollowing("name");
                CharacterToMap.Powers.Add(reader.ReadElementContentAsString());
                reader.Read();
            }
        }

        private static void ParsePublisher(XmlReader reader)
        {
            if (reader.Name != "publisher") { reader.ReadToFollowing("publisher"); }
            reader.ReadToDescendant("id");
            CharacterToMap.PublisherId = reader.ReadElementContentAsInt();
        }

        private static void ParseRealName(XmlReader reader)
        {
            if (reader.Name != "real_name") { reader.ReadToFollowing("real_name"); }
            CharacterToMap.RealName = reader.ReadElementContentAsString();
        }

        private static void ParseComicVineSiteUrl(XmlReader reader)
        {
            if (reader.Name != "site_detail_url") { reader.ReadToFollowing("site_detail_url"); }
            CharacterToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseTeamsMemberOf(XmlReader reader)
        {
            if (reader.Name != "teams") { reader.ReadToFollowing("teams"); }
            while (reader.Read())
            {
                if (reader.Name == "team" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    CharacterToMap.TeamIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "teams" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }
    }
}
