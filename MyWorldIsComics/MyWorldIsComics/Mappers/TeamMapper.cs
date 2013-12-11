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
                if (reader.Name == "response") reader.ReadToFollowing("results");

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
                //ParseEnemies(reader);
                //ParseFriends(reader);
                //ParseMembers(reader);
                _teamToMap = GenericResourceMapper.ParseDeck(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseDescriptionString(reader, _teamToMap) as Team;
                //ParseIssuesDispandedIn(reader);
                ParseFirstAppearance(reader);
                _teamToMap = GenericResourceMapper.ParseId(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseImage(reader, _teamToMap) as Team;
                _teamToMap = GenericResourceMapper.ParseName(reader, _teamToMap) as Team;
            }
            return _teamToMap;
        }

        public Team MapFilteredXmlObject(Team basicTeam, string filteredTeamString, string filter)
        {
            using (XmlReader readerInit = XmlReader.Create(new StringReader(filteredTeamString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(readerInit)) return _teamToMap;
                _teamToMap = basicTeam;
                switch (filter)
                {
                    case "first_appeared_in_issue":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseFirstAppearance(reader);
                        }
                        break;
                    case "disbanded_in_issues":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseIssuesDispandedIn(reader);
                        }
                        break;
                    case "characters":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseMembers(reader);
                        }
                        break;
                    case "character_enemies":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseEnemies(reader);
                        }
                        break;
                    case "character_friends":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseFriends(reader);
                        }
                        break;
                }
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
            if (reader.Name == "character_enemies" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _teamToMap.EnemyIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "character_enemies" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseFriends(XmlReader reader)
        {
            if (reader.Name != "character_friends") { reader.ReadToFollowing("character_friends"); }
            if (reader.Name == "character_friends" && reader.IsEmptyElement) return;
            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _teamToMap.FriendIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "character_friends" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseMembers(XmlReader reader)
        {
            if (reader.Name != "characters") { reader.ReadToFollowing("characters"); }
            if (reader.Name == "characters" && reader.IsEmptyElement) return;
            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _teamToMap.MemberIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "characters" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
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
            if (reader.Name == "disbanded_in_issues" && reader.IsEmptyElement) return;
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
            if (reader.Name == "movies" && reader.IsEmptyElement) return;
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
