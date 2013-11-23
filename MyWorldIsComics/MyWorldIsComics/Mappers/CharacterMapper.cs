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

    class CharacterMapper
    {
        private static Character characterToMap = new Character();

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
            character = characterToMap;
        }

        public static void MapXmlObject(string xmlString, out Character character)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseAliases(reader);
                ParseApiUrl(reader);
                ParseBirth(reader);
                ParseEnemies(reader);
                ParseFriends(reader);
                ParseAppearanceCount(reader);
                ParseCreators(reader);
                ParseDeck(reader);
                ParseDescriptionString(reader);
                ParseFirstAppearance(reader);
                ParseGender(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseIssuesDiedIn(reader);
                ParseMovies(reader);
                ParseName(reader);
                ParseOrigin(reader);
                ParsePowers(reader);
                ParsePublisher(reader);
                ParseRealName(reader);
                ParseSiteUrl(reader);
                ParseTeamsMemberOf(reader);
            }
            character = characterToMap;
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

        private static void ParseEnemies(XmlReader reader)
        {
            List<Character> enemies = new List<Character>();
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToFollowing("character");
                reader.Read();
                Character character = new Character
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Name = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                enemies.Add(character);
            }
            characterToMap.Enemies = enemies;
        }

        private static void ParseFriends(XmlReader reader)
        {
            reader.ReadToFollowing("character_friends");
            List<Character> friends = new List<Character>();
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToFollowing("character");
                reader.Read();
                Character character = new Character
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Name = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                friends.Add(character);
            }
            characterToMap.Friends = friends;
        }

        private static void ParseAppearanceCount(XmlReader reader)
        {
            if (reader.Name != "count_of_issue_appearances")
            {
                reader.ReadToFollowing("count_of_issue_appearances");
            }

            characterToMap.IssueAppearancesCount = reader.ReadElementContentAsInt();
        }

        private static void ParseCreators(XmlReader reader)
        {
            reader.ReadToFollowing("creator");
            List<Creator> creators = new List<Creator>();
            while (reader.Name == "creator")
            {
                reader.ReadToFollowing("creator");
                reader.Read();
                Creator creator = new Creator
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Name = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                creators.Add(creator);
                reader.Read();
            }

            characterToMap.Creators = creators;
        }

        private static void ParseDeck(XmlReader reader)
        {
            if (reader.Name != "deck") { reader.ReadToFollowing("deck"); }
            characterToMap.Deck = reader.ReadElementContentAsString();
        }

        private static void ParseDescriptionString(XmlReader reader)
        {
            if (reader.Name != "description") { reader.ReadToFollowing("description"); }
            characterToMap.DescriptionString = reader.ReadElementContentAsString();
        }

        private static void ParseFirstAppearance(XmlReader reader)
        {
            if (reader.Name != "first_appeared_in_issue") { reader.ReadToFollowing("first_appeared_in_issue"); }
            reader.Read();
            var issue = new Issue
            {
                ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                UniqueId = reader.ReadElementContentAsInt(),
                Title = reader.ReadElementContentAsString(),
                IssueNumber = reader.ReadElementContentAsInt()
            };

            characterToMap.FirstAppearance = issue;
        }

        private static void ParseGender(XmlReader reader)
        {
            reader.ReadToFollowing("gender");
            characterToMap.Gender = Gender.GetGender(reader.ReadElementContentAsInt());
        }

        private static void ParseId(XmlReader reader)
        {
            characterToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private static void ParseImage(XmlReader reader)
        {
            reader.ReadToFollowing("super_url");
            characterToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseIssuesDiedIn(XmlReader reader)
        {
            reader.ReadToFollowing("issues_died_in");
            reader.Read();
            List<Issue> deathIssues = new List<Issue>();
            while (reader.Name == "issue")
            {
                reader.ReadToFollowing("issue");
                reader.Read();
                Issue issue = new Issue
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Title = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                deathIssues.Add(issue);
                reader.Read();
            }

            characterToMap.IssuesDiedIn = deathIssues;
        }

        private static void ParseMovies(XmlReader reader)
        {
            reader.ReadToFollowing("movies");
            List<Movie> movies = new List<Movie>();
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToFollowing("api_detail_url");
                Movie movie = new Movie
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Title = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                movies.Add(movie);

                reader.Read();
                if (reader.Name != "movie") { i = 10; }
            }
            characterToMap.Movies = movies;
        }

        private static void ParseName(XmlReader reader)
        {
            reader.ReadToFollowing("name");
            characterToMap.Name = reader.ReadElementContentAsString();
        }

        private static void ParseOrigin(XmlReader reader)
        {
            if (reader.Name == "origin") { reader.Read(); }
            else { reader.ReadToFollowing("origin"); }

            var origin = new Origin
            {
                ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                UniqueId = reader.ReadElementContentAsInt(),
                Name = reader.ReadElementContentAsString()
            };

            characterToMap.Origin = origin;
        }

        private static void ParsePowers(XmlReader reader)
        {
            reader.ReadToFollowing("powers");
            reader.Read();

            List<String> powers = new List<String>();
            while (reader.Name == "power")
            {
                reader.ReadToFollowing("name");
                powers.Add(reader.ReadElementContentAsString());
                reader.Read();
            }
            characterToMap.Powers.AddRange(powers);
        }

        private static void ParsePublisher(XmlReader reader)
        {
            reader.ReadToFollowing("publisher");
            reader.Read();
            var publisher = new Publisher
            {
                ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                UniqueId = reader.ReadElementContentAsInt(),
                Name = reader.ReadElementContentAsString()
            };

            characterToMap.Publisher = publisher;
        }

        private static void ParseRealName(XmlReader reader)
        {
            reader.ReadToFollowing("real_name");
            characterToMap.RealName = reader.ReadElementContentAsString();
        }

        private static void ParseSiteUrl(XmlReader reader)
        {
            characterToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());
        }

        private static void ParseTeamsMemberOf(XmlReader reader)
        {
            reader.ReadToFollowing("teams");
            List<Team> teams = new List<Team>();
            for (int i = 0; i < 10; i++)
            {
                reader.ReadToFollowing("team");
                reader.Read();
                Team team = new Team
                {
                    ComicVineApiUrl = new Uri(reader.ReadElementContentAsString()),
                    UniqueId = reader.ReadElementContentAsInt(),
                    Name = reader.ReadElementContentAsString(),
                    ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString())
                };

                teams.Add(team);
            }
            characterToMap.TeamsMemberOf = teams;
        }

        
    }
}
