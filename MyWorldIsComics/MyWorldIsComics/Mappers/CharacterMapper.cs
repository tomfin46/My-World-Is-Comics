using System.Diagnostics.Contracts;
using System.Xml.Serialization;
using MyWorldIsComics.DataModel.Interfaces;

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

    using MyWorldIsComics.DataSource;

    #endregion

    class CharacterMapper
    {
        // ReSharper disable once InconsistentNaming
        private Character _characterToMap;

        public CharacterMapper()
        {
            _characterToMap = new Character();
        }

        public Character QuickMapXmlObject(string quickCharacterString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(quickCharacterString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _characterToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                _characterToMap = GenericResourceMapper.ParseDeck(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseId(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseImage(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseName(reader, _characterToMap) as Character;
            }
            return _characterToMap;
        }

        public Character MapSearchXmlObject(string characterSearchString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(characterSearchString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _characterToMap;

                reader.ReadToFollowing("results");
                ParseAliases(reader);
                reader.ReadToFollowing("birth");
                ParseBirth(reader);
                ParseAppearanceCount(reader);
                _characterToMap = GenericResourceMapper.ParseDeck(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseDescriptionString(reader, _characterToMap) as Character;
                ParseFirstAppearance(reader);
                ParseGender(reader);
                _characterToMap = GenericResourceMapper.ParseId(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseImage(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseName(reader, _characterToMap) as Character;
                ParsePublisher(reader);
                ParseRealName(reader);
            }
            return _characterToMap;
        }

        public Character MapCharacterXmlObject(string characterSearchString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(characterSearchString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Character { Name = ServiceConstants.QueryNotFound };

                reader.ReadToFollowing("results");
                ParseAliases(reader);
                ParseBirth(reader);
                ParseAppearanceCount(reader);
                _characterToMap = GenericResourceMapper.ParseDeck(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseDescriptionString(reader, _characterToMap) as Character;
                ParseFirstAppearance(reader);
                _characterToMap = GenericResourceMapper.ParseId(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseImage(reader, _characterToMap) as Character;
                _characterToMap = GenericResourceMapper.ParseName(reader, _characterToMap) as Character;
                ParseRealName(reader);
                ParseTeamsMemberOf(reader);
            }
            return _characterToMap;
        }

        public List<string> GetSuggestionsList(string suggestionsString)
        {
            List<string> names = new List<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new List<string>();
                reader.ReadToFollowing("results");

                if (reader.Name != "character") { reader.ReadToFollowing("character"); }
                do
                {
                    if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                    {
                        
                        reader.ReadToFollowing("name");
                        var name = reader.ReadElementContentAsString();
                        names.Add(name);
                    }
                    else if (reader.Name == "results" && reader.NodeType == XmlNodeType.EndElement)
                    {
                        return names;
                    }
                }
                while (reader.Read());
            }
            return names;
        }

        private void ParseAliases(XmlReader reader)
        {
            _characterToMap.Aliases.Clear();
            if (reader.Name != "aliases") { reader.ReadToFollowing("aliases"); }
            var aliases = reader.ReadElementContentAsString().Split('\n');
            _characterToMap.Aliases.AddRange(aliases);
        }

        private void ParseBirth(XmlReader reader)
        {
            if (reader.Name != "birth") { reader.ReadToFollowing("birth"); }
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }

            var commaPos = date.IndexOf(',');
            var year = int.Parse(date.Substring(commaPos + 1));
            var month = Month.GetMonthInt(date.Substring(0, 3));
            var day = int.Parse(date.Substring(4, commaPos - 4));

            _characterToMap.Birth = new DateTime(year, month, day);
        }

        private void ParseEnemies(XmlReader reader)
        {
            if (reader.Name != "character_enemies") { reader.ReadToFollowing("character_enemies"); }
            reader.ReadToDescendant("character");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _characterToMap.EnemyIds.Add(reader.ReadElementContentAsInt());

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
                _characterToMap.FriendIds.Add(reader.ReadElementContentAsInt());

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
            _characterToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private void ParseCreators(XmlReader reader)
        {
            if (reader.Name != "creators") { reader.ReadToFollowing("creators"); }
            reader.Read();
            while (reader.Name == "creator")
            {
                reader.ReadToDescendant("id");
                _characterToMap.CreatorIds.Add(reader.ReadElementContentAsInt());

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
            if (reader.IsEmptyElement) return; 
            reader.ReadToDescendant("id");
            _characterToMap.FirstAppearanceId = reader.ReadElementContentAsInt();
        }

        private void ParseGender(XmlReader reader)
        {
            if (reader.Name != "gender") { reader.ReadToFollowing("gender"); }
            _characterToMap.Gender = Gender.GetGender(reader.ReadElementContentAsInt());
        }

        private void ParseIssuesDiedIn(XmlReader reader)
        {
            if (reader.Name != "issues_died_in") { reader.ReadToFollowing("issues_died_in"); }
            reader.Read();
            while (reader.Name == "issue")
            {
                reader.ReadToDescendant("id");
                _characterToMap.DeathIssueIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
            }
        }

        private void ParseMovies(XmlReader reader)
        {
            if (reader.Name != "movies") { reader.ReadToFollowing("movies"); }
            reader.ReadToDescendant("movie");

            for (int i = 0; i < 10; i++)
            {
                reader.ReadToDescendant("id");
                _characterToMap.MovieIds.Add(reader.ReadElementContentAsInt());

                reader.ReadToFollowing("site_detail_url");
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                if (reader.Name != "movie") { i = 10; }
            }
        }

       private void ParseOrigin(XmlReader reader)
        {
            if (reader.Name != "origin") { reader.ReadToFollowing("origin"); }
            reader.ReadToFollowing("name");
            _characterToMap.Origin = reader.ReadElementContentAsString();
        }

        private void ParsePowers(XmlReader reader)
        {
            if (reader.Name != "powers") { reader.ReadToFollowing("powers"); }
            reader.Read();

            while (reader.Name == "power")
            {
                reader.ReadToFollowing("name");
                _characterToMap.Powers.Add(reader.ReadElementContentAsString());
                reader.Read();
            }
        }

        private void ParsePublisher(XmlReader reader)
        {
            if (reader.Name != "publisher") { reader.ReadToFollowing("publisher"); }
            reader.ReadToDescendant("id");
            _characterToMap.PublisherId = reader.ReadElementContentAsInt();
        }

        private void ParseRealName(XmlReader reader)
        {
            if (reader.Name != "real_name") { reader.ReadToFollowing("real_name"); }
            _characterToMap.RealName = reader.ReadElementContentAsString();
        }

        private void ParseTeamsMemberOf(XmlReader reader)
        {
            if (reader.Name != "teams") { reader.ReadToFollowing("teams"); }
            while (reader.Read())
            {
                if (reader.Name == "team" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _characterToMap.TeamIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "teams" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

    }
}
