﻿namespace MyWorldIsComics.Mappers
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
        private Team _teamToMap;

        public TeamMapper()
        {
            _teamToMap = new Team();
        }

        public Team QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _teamToMap;
                reader.ReadToFollowing("results");
                _teamToMap = GenericResourceMapper.ParseDeck(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseId(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseImage(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseName(reader, _teamToMap) as Team;
                _teamToMap.ResourceString = xmlString;
            }
            return _teamToMap;
        }

        public Team MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _teamToMap;
                reader.ReadToFollowing("results");
                ParseAliases(reader);
                _teamToMap = GenericResourceMapper.ParseComicVineApiUrl(reader, _teamToMap) as Team;
                ParseEnemies(reader);
                ParseFriends(reader);
                ParseMembers(reader);
                ParseAppearanceCount(reader);
                ParseMembersCount(reader);
                _teamToMap = GenericResourceMapper.ParseDeck(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseDescriptionString(reader, _teamToMap) as Team;
                ParseIssuesDispandedIn(reader);
                ParseFirstAppearance(reader);
                _teamToMap = GenericResourceMapper.ParseId(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseImage(reader, _teamToMap) as Team;
                ParseMovies(reader);
                _teamToMap = GenericResourceMapper.ParseName(reader, _teamToMap) as Team;
                ParsePublisher(reader);
                _teamToMap = GenericResourceMapper.ParseComicVineSiteUrl(reader, _teamToMap) as Team;
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

        private void ParsePublisher(XmlReader reader)
        {
            if (reader.Name != "publisher") { reader.ReadToFollowing("publisher"); }
            reader.ReadToDescendant("id");
            _teamToMap.PublisherId = reader.ReadElementContentAsInt();
        }
    }
}
