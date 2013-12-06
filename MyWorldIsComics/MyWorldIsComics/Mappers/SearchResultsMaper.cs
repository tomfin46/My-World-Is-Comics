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
    class SearchResultsMaper
    {
        private List<Character> characterResults;
        private List<Concept> conceptResults;
        private List<Creator> creatorResults;
        private List<Issue> issueResults;
        private List<Location> locationResults;
        private List<Movie> movieResults;
        private List<ObjectResource> objectResults;
        private List<Publisher> publisherResults;
        private List<StoryArc> storyArcResults;
        private List<Team> teamResults;
        private List<Volume> volumeResults;

        public SearchResultsMaper()
        {
            characterResults = new List<Character>();
            conceptResults = new List<Concept>();
            creatorResults = new List<Creator>();
            issueResults = new List<Issue>();
            locationResults = new List<Location>();
            movieResults = new List<Movie>();
            objectResults = new List<ObjectResource>();
            publisherResults = new List<Publisher>();
            storyArcResults = new List<StoryArc>();
            teamResults = new List<Team>();
            volumeResults = new List<Volume>();
        }

        public void MapSearchResults(string searchResults)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(searchResults)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return ;
                reader.ReadToFollowing("results");
                reader.Read();
                switch (reader.Name)
                {
                    case "character":
                        characterResults.Add(new CharacterMapper().QuickMapXmlObject(searchResults));
                        break;
                    case "concept":
                        conceptResults.Add(new ConceptMapper().QuickMapXmlObject(searchResults));
                        break;
                    case "issue":
                        issueResults.Add(new IssueMapper().QuickMapXmlObject(searchResults));
                        break;
                    case "location":
                        locationResults.Add(new LocationMapper().QuickMapXmlObject(searchResults));
                        break;
                    case "object":
                        objectResults.Add(new ObjectMapper().QuickMapXmlObject(searchResults));
                        break;
                    case "team":
                        teamResults.Add(new TeamMapper().QuickMapXmlObject(searchResults));
                        break;
                }
            }
        }
    }
}
