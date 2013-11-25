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

    class TeamMapper
    {
        private readonly Team _teamToMap;

        public TeamMapper()
        {
            _teamToMap = new Team();
        }
        public Team QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseDeck(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseName(reader);
                _teamToMap.ResourceString = xmlString;
            }
            return _teamToMap;
        }

        public Team MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseAliases(reader);
                ParseComicVineApiUrl(reader);
                ParseEnemies(reader);
                ParseFriends(reader);
                ParseMembers(reader);
                ParseAppearanceCount(reader);
                ParseMembersCount(reader);
                ParseDeck(reader);
                ParseDescriptionString(reader);
                ParseIssuesDispandedIn(reader);
                ParseFirstAppearance(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseMovies(reader);
                ParseName(reader);
                ParsePublisher(reader);
                ParseComicVineSiteUrl(reader);
            }
            return _teamToMap;
        }

        private void ParseAliases(XmlReader reader)
        {
            _teamToMap.Aliases.Clear();
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n'); 
            _teamToMap.Aliases.AddRange(aliases);
        }

        private void ParseComicVineApiUrl(XmlReader reader)
        {
            if (reader.Name != "api_detail_url") { reader.ReadToFollowing("api_detail_url"); }
            _teamToMap.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());
        }

        private void ParseEnemies(XmlReader reader)
        {
            if (reader.Name != "character_enemies") { reader.ReadToFollowing("character_enemies"); }
            reader.ReadToDescendant("character");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _teamToMap.EnemyIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "character") { i = 10; }
            }
        }

        private void ParseFriends(XmlReader reader)
        {
            if (reader.Name != "character_friends") { reader.ReadToFollowing("character_friends"); }
            reader.ReadToDescendant("character");
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _teamToMap.FriendIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "character") { i = 10; }
            }
        }

        private void ParseMembers(XmlReader reader)
        {
            if (reader.Name != "characters") { reader.ReadToFollowing("characters"); }
            reader.ReadToDescendant("character");
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _teamToMap.MemberIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "character") { i = 10; }
            }
        }

        private void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issue_appearances") { reader.ReadToFollowing("count_of_issue_appearances"); }
            _teamToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }
        private void ParseMembersCount(XmlReader reader)
        {
            if (reader.Name != "count_of_team_members") { reader.ReadToFollowing("count_of_team_members"); }
            _teamToMap.MembersCount = reader.ReadElementContentAsInt();
        }

        private void ParseDeck(XmlReader reader)
        {
            if (reader.Name != "deck") { reader.ReadToFollowing("deck"); }
            _teamToMap.Deck = reader.ReadElementContentAsString();
        }

        private void ParseDescriptionString(XmlReader reader)
        {
            if (reader.Name != "description") { reader.ReadToFollowing("description"); }
            _teamToMap.DescriptionString = reader.ReadElementContentAsString();
        }
        private void ParseIssuesDispandedIn(XmlReader reader)
        {
            if (reader.Name != "disbanded_in_issues") { reader.ReadToFollowing("disbanded_in_issues"); }
            reader.Read();
            while (reader.Name == "issue")
            {
                reader.ReadToDescendant("id");
                _teamToMap.IssuesDispandedInIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
            }
        }

        private void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            reader.ReadToDescendant("id");
            _teamToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }

        private void ParseId(XmlReader reader)
        {
            if (reader.Name != "id")
            {
                reader.ReadToFollowing("first_appeared_in_issue");
                reader.ReadToNextSibling("id");
            }
            _teamToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private void ParseImage(XmlReader reader)
        {
            if (reader.Name != "image") { reader.ReadToFollowing("image"); }
            if (reader.IsEmptyElement) return;

            reader.ReadToFollowing("super_url");
            _teamToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }

        private void ParseMovies(XmlReader reader)
        {
            if (reader.Name != "movies") { reader.ReadToFollowing("movies"); }
            reader.ReadToDescendant("movie");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _teamToMap.MovieIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "movie") { i = 10; }
            }
        }

        private void ParseName(XmlReader reader)
        {
            if (reader.Name != "name" && reader.Name != "movies")
            {
                reader.ReadToFollowing("movies");
                reader.ReadToNextSibling("name");
            }
            else
            {
                reader.ReadToFollowing("name");
            }

            _teamToMap.Name = reader.ReadElementContentAsString();
        }

        private void ParsePublisher(XmlReader reader)
        {
            if (reader.Name != "publisher") { reader.ReadToFollowing("publisher"); }
            reader.ReadToDescendant("id");
            _teamToMap.PublisherId = reader.ReadElementContentAsInt();
        }

        private void ParseComicVineSiteUrl(XmlReader reader)
        {
            if (reader.Name != "site_detail_url") { reader.ReadToFollowing("site_detail_url"); }
            _teamToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());
        }
    }
}
