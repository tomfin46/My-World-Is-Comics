using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Interfaces;
using MyWorldIsComics.DataModel.Resources;

namespace MyWorldIsComics.Mappers
{
    class SearchResultsMapper
    {
        public ObservableCollection<Results> Results { get; private set; }
        public Results CharacterResults { get; private set; }
        public Results ConceptResults { get; private set; }
        public Results CreatorResults { get; private set; }
        public Results IssueResults { get; private set; }
        public Results LocationResults { get; private set; }
        public Results MovieResults { get; private set; }
        public Results ObjectResults { get; private set; }
        public Results PublisherResults { get; private set; }
        public Results StoryArcResults { get; private set; }
        public Results TeamResults { get; private set; }
        public Results VolumeResults { get; private set; }

        public SearchResultsMapper()
        {
            Results = new ObservableCollection<Results>();
            CharacterResults = new Results { Name = "Characters", ResultsList = new ObservableCollection<IResource>() };
            ConceptResults = new Results { Name = "Concepts", ResultsList = new ObservableCollection<IResource>() };
            CreatorResults = new Results { Name = "Creators", ResultsList = new ObservableCollection<IResource>() };
            IssueResults = new Results { Name = "Issues", ResultsList = new ObservableCollection<IResource>() };
            LocationResults = new Results { Name = "Locations", ResultsList = new ObservableCollection<IResource>() };
            MovieResults = new Results { Name = "Movies", ResultsList = new ObservableCollection<IResource>() };
            ObjectResults = new Results { Name = "Objects", ResultsList = new ObservableCollection<IResource>() };
            PublisherResults = new Results { Name = "Publishers", ResultsList = new ObservableCollection<IResource>() };
            StoryArcResults = new Results { Name = "Story Arcs", ResultsList = new ObservableCollection<IResource>() };
            TeamResults = new Results { Name = "Teams", ResultsList = new ObservableCollection<IResource>() };
            VolumeResults = new Results { Name = "Volumes", ResultsList = new ObservableCollection<IResource>() };
            FillResults();
        }

        public void MapSearchResults(string searchResults)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(searchResults)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return;
                reader.ReadToFollowing("results");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement) continue;
                    switch (reader.Name)
                    {
                        case "character":
                            MapCharacter(reader);
                            break;
                        case "concept":
                            MapConcepts(reader);
                            break;
                        case "issue":
                            MapIssues(reader);
                            break;
                        case "location":
                            MapLocations(reader);
                            break;
                        case "movie":
                            MapMovie(reader);
                            break;
                        case "object":
                            MapObjects(reader);
                            break;
                        case "person":
                            MapCreators(reader);
                            break;
                        case "publisher":
                            MapPublishers(reader);
                            break;
                        case "story_arc":
                            MapStoryArc(reader);
                            break;
                        case "team":
                            MapTeam(reader);
                            break;
                        case "volume":
                            MapVolume(reader);
                            break;
                    }
                }
            }
        }

        private void FillResults()
        {
            Results.Add(CharacterResults);
            Results.Add(ConceptResults);
            Results.Add(CreatorResults);
            Results.Add(IssueResults);
            Results.Add(LocationResults);
            Results.Add(MovieResults);
            Results.Add(ObjectResults);
            Results.Add(PublisherResults);
            Results.Add(StoryArcResults);
            Results.Add(TeamResults);
            Results.Add(VolumeResults);
        }

        private void MapVolume(XmlReader reader)
        {
            Volume volumeToMap = new Volume();
            volumeToMap = GenericResourceMapper.ParseDeck(reader, volumeToMap) as Volume;
            volumeToMap = GenericResourceMapper.ParseId(reader, volumeToMap) as Volume;
            volumeToMap = GenericResourceMapper.ParseImage(reader, volumeToMap) as Volume;
            volumeToMap = GenericResourceMapper.ParseName(reader, volumeToMap) as Volume;
            VolumeResults.ResultsList.Add(volumeToMap);
        }

        private void MapTeam(XmlReader reader)
        {
            Team teamToMap = new Team();
            teamToMap = GenericResourceMapper.ParseDeck(reader, teamToMap) as Team;
            teamToMap = GenericResourceMapper.ParseId(reader, teamToMap) as Team;
            teamToMap = GenericResourceMapper.ParseImage(reader, teamToMap) as Team;
            teamToMap = GenericResourceMapper.ParseName(reader, teamToMap) as Team;
            TeamResults.ResultsList.Add(teamToMap);
        }

        private void MapStoryArc(XmlReader reader)
        {
            StoryArc storyArcToMap = new StoryArc();
            storyArcToMap = GenericResourceMapper.ParseDeck(reader, storyArcToMap) as StoryArc;
            storyArcToMap = GenericResourceMapper.ParseId(reader, storyArcToMap) as StoryArc;
            storyArcToMap = GenericResourceMapper.ParseImage(reader, storyArcToMap) as StoryArc;
            storyArcToMap = GenericResourceMapper.ParseName(reader, storyArcToMap) as StoryArc;
            StoryArcResults.ResultsList.Add(storyArcToMap);
        }
        
        private void MapPublishers(XmlReader reader)
        {
            Publisher publisherToMap = new Publisher();
            publisherToMap = GenericResourceMapper.ParseDeck(reader, publisherToMap) as Publisher;
            publisherToMap = GenericResourceMapper.ParseId(reader, publisherToMap) as Publisher;
            publisherToMap = GenericResourceMapper.ParseImage(reader, publisherToMap) as Publisher;
            publisherToMap = GenericResourceMapper.ParseName(reader, publisherToMap) as Publisher;
            PublisherResults.ResultsList.Add(publisherToMap);
        }

        private void MapCreators(XmlReader reader)
        {
            Creator creatorToMap = new Creator();
            creatorToMap = GenericResourceMapper.ParseDeck(reader, creatorToMap) as Creator;
            creatorToMap = GenericResourceMapper.ParseId(reader, creatorToMap) as Creator;
            creatorToMap = GenericResourceMapper.ParseImage(reader, creatorToMap) as Creator;
            creatorToMap = GenericResourceMapper.ParseName(reader, creatorToMap) as Creator;
            CreatorResults.ResultsList.Add(creatorToMap);
        }

        private void MapObjects(XmlReader reader)
        {
            ObjectResource objectToMap = new ObjectResource();
            objectToMap = GenericResourceMapper.ParseDeck(reader, objectToMap) as ObjectResource;
            objectToMap = GenericResourceMapper.ParseId(reader, objectToMap) as ObjectResource;
            objectToMap = GenericResourceMapper.ParseImage(reader, objectToMap) as ObjectResource;
            objectToMap = GenericResourceMapper.ParseName(reader, objectToMap) as ObjectResource;
            ObjectResults.ResultsList.Add(objectToMap);
        }

        private void MapMovie(XmlReader reader)
        {
            Movie movieToMap = new Movie();
            movieToMap = GenericResourceMapper.ParseDeck(reader, movieToMap) as Movie;
            movieToMap = GenericResourceMapper.ParseId(reader, movieToMap) as Movie;
            movieToMap = GenericResourceMapper.ParseImage(reader, movieToMap) as Movie;
            movieToMap = GenericResourceMapper.ParseName(reader, movieToMap) as Movie;
            MovieResults.ResultsList.Add(movieToMap);
        }

        private void MapLocations(XmlReader reader)
        {
            Location locationToMap = new Location();
            locationToMap = GenericResourceMapper.ParseDeck(reader, locationToMap) as Location;
            locationToMap = GenericResourceMapper.ParseId(reader, locationToMap) as Location;
            locationToMap = GenericResourceMapper.ParseImage(reader, locationToMap) as Location;
            locationToMap = GenericResourceMapper.ParseName(reader, locationToMap) as Location;
            LocationResults.ResultsList.Add(locationToMap);
        }

        private void MapIssues(XmlReader reader)
        {
            Issue issueToMap = new Issue();
            issueToMap = GenericResourceMapper.ParseDeck(reader, issueToMap) as Issue;
            issueToMap = GenericResourceMapper.ParseId(reader, issueToMap) as Issue;
            issueToMap = GenericResourceMapper.ParseImage(reader, issueToMap) as Issue;
            issueToMap = GenericResourceMapper.ParseName(reader, issueToMap) as Issue;
            IssueResults.ResultsList.Add(issueToMap);
        }

        private void MapConcepts(XmlReader reader)
        {
            Concept conceptToMap = new Concept();
            conceptToMap = GenericResourceMapper.ParseDeck(reader, conceptToMap) as Concept;
            conceptToMap = GenericResourceMapper.ParseId(reader, conceptToMap) as Concept;
            conceptToMap = GenericResourceMapper.ParseImage(reader, conceptToMap) as Concept;
            conceptToMap = GenericResourceMapper.ParseName(reader, conceptToMap) as Concept;
            ConceptResults.ResultsList.Add(conceptToMap);
        }

        private void MapCharacter(XmlReader reader)
        {
            Character characterToMap = new Character();
            characterToMap = GenericResourceMapper.ParseDeck(reader, characterToMap) as Character;
            characterToMap = GenericResourceMapper.ParseId(reader, characterToMap) as Character;
            characterToMap = GenericResourceMapper.ParseImage(reader, characterToMap) as Character;
            characterToMap = GenericResourceMapper.ParseName(reader, characterToMap) as Character;
            CharacterResults.ResultsList.Add(characterToMap);
        }
    }
}
