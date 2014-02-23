using System.Collections.ObjectModel;
using System.Reflection;
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Interfaces;
using MyWorldIsComics.DataModel.ResponseSchemas;

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
            CharacterResults = new Results { Name = "Characters", ResultsList = new ObservableCollection<IResponse>() };
            ConceptResults = new Results { Name = "Concepts", ResultsList = new ObservableCollection<IResponse>() };
            CreatorResults = new Results { Name = "Creators", ResultsList = new ObservableCollection<IResponse>() };
            IssueResults = new Results { Name = "Issues", ResultsList = new ObservableCollection<IResponse>() };
            LocationResults = new Results { Name = "Locations", ResultsList = new ObservableCollection<IResponse>() };
            MovieResults = new Results { Name = "Movies", ResultsList = new ObservableCollection<IResponse>() };
            ObjectResults = new Results { Name = "Objects", ResultsList = new ObservableCollection<IResponse>() };
            PublisherResults = new Results { Name = "Publishers", ResultsList = new ObservableCollection<IResponse>() };
            StoryArcResults = new Results { Name = "Story Arcs", ResultsList = new ObservableCollection<IResponse>() };
            TeamResults = new Results { Name = "Teams", ResultsList = new ObservableCollection<IResponse>() };
            VolumeResults = new Results { Name = "Volumes", ResultsList = new ObservableCollection<IResponse>() };
            FillResults();
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

        public void MapSearchResults(string searchResults)
        {
            var json = JsonDeserialize.DeserializeJsonString<JsonMultipleBase>(searchResults);

            foreach (var response in json.Results)
            {
                switch (response.Resource_Type)
                {
                    case "character":
                        MapCharacter(response);
                        break;
                    case "concept":
                        MapConcept(response);
                        break;
                    case "issue":
                        MapIssue(response);
                        break;
                    case "location":
                        MapLocation(response);
                        break;
                    case "movie":
                        MapMovie(response);
                        break;
                    case "object":
                        MapObject(response);
                        break;
                    case "person":
                        MapCreator(response);
                        break;
                    case "publisher":
                        MapPublisher(response);
                        break;
                    case "story_arc":
                        MapStoryArc(response);
                        break;
                    case "team":
                        MapTeam(response);
                        break;
                    case "volume":
                        MapVolume(response);
                        break;
                }
            }
        }

        private void MapCharacter(ResponseSchema response)
        {
            var character = ResponseSchemaToT<Character>(response);
            CharacterResults.ResultsList.Add(character);
        }

        private void MapVolume(ResponseSchema response)
        {
            var volume = ResponseSchemaToT<Volume>(response);
            VolumeResults.ResultsList.Add(volume);
        }

        private void MapTeam(ResponseSchema response)
        {
            var team = ResponseSchemaToT<Team>(response);
            TeamResults.ResultsList.Add(team);
        }

        private void MapStoryArc(ResponseSchema response)
        {
            var storyArc = ResponseSchemaToT<StoryArc>(response);
            StoryArcResults.ResultsList.Add(storyArc);
        }

        private void MapPublisher(ResponseSchema response)
        {
            var publisher = ResponseSchemaToT<Publisher>(response);
            PublisherResults.ResultsList.Add(publisher);
        }

        private void MapCreator(ResponseSchema response)
        {
            var creator = ResponseSchemaToT<Person>(response);
            CreatorResults.ResultsList.Add(creator);
        }

        private void MapObject(ResponseSchema response)
        {
            var obj = ResponseSchemaToT<ObjectResource>(response);
            ObjectResults.ResultsList.Add(obj);
        }

        private void MapMovie(ResponseSchema response)
        {
            var movie = ResponseSchemaToT<Movie>(response);
            MovieResults.ResultsList.Add(movie);
        }

        private void MapLocation(ResponseSchema response)
        {
            var location = ResponseSchemaToT<Location>(response);
            LocationResults.ResultsList.Add(location);
        }

        private void MapIssue(ResponseSchema response)
        {
            var issue = ResponseSchemaToT<Issue>(response);
            IssueResults.ResultsList.Add(issue);
        }

        private void MapConcept(ResponseSchema response)
        {
            var concept = ResponseSchemaToT<Concept>(response);
            ConceptResults.ResultsList.Add(concept);
        }

        private T ResponseSchemaToT<T>(ResponseSchema response) where T : IResponse, new()
        {
            var iResponse = new T();
            var iRespType = iResponse.GetType();

            var respType = response.GetType();
            var props = respType.GetRuntimeProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(response);

                if (value == null) continue;
                if (prop.SetMethod == null) continue;

                var iProp = iRespType.GetRuntimeProperty(prop.Name);
                iProp.SetValue(iResponse, value);
            }

            return iResponse;
        }
    }
}
