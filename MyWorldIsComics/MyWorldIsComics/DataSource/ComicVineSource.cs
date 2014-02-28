using MyWorldIsComics.DataModel.ResponseSchemas;

namespace MyWorldIsComics.DataSource
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http;
    using DataModel.Enums;
    using Mappers;

    using DataModel.DescriptionContent;


    class ComicVineSource
    {
        private static readonly ComicVineSource comicVineSource = new ComicVineSource();
        private static HttpClient client;
        private static CancellationTokenSource cts;

        private static Dictionary<string, int> characterExceptions = new Dictionary<string, int>
        {
            { "Hulk", 2267 }, { "Thor", 2268 }, { "Angela", 11761 }, { "Death", 42501 }, { "Stephen Strange", 1456 }, { "Toxin", 6733 },
            {"Natalia Romanova", 3200}, {"Thane", 92620}, {"Taneleer Tivan", 13987}, {"Trevor Slattery", 3530}, {"Skye", -1}
        };

        public ComicVineSource()
        {
            client = new HttpClient();
            cts = new CancellationTokenSource();
        }

        public static async Task<string> ExecuteSearchAsync(string query)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query, new List<string> { "deck", "id", "image", "issue_number", "name", "publisher", "volume" }));
        }
        public static async Task<string> ExecuteSearchAsync(string query, string filter)
        {
            var uri = comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query,
                new List<string> {"deck", "id", "image", "issue_number", "name", "publisher", "volume"});
            uri = new Uri(uri.AbsoluteUri + "&resources=" + filter);
            return await QueryServiceAsync(uri);
        }

        public static async Task<string> ExecuteCharacterFilterLimitOneAsync(string query)
        {
            if (characterExceptions.ContainsKey(query))
            {
                return await GetQuickCharacterAsync(characterExceptions[query]);
            }

            var uri = comicVineSource.ContructUrl(Resources.ResourcesEnum.Characters, query, new List<string> { "deck", "id", "image", "name", "publisher" });
            uri = new Uri(uri.AbsoluteUri.Replace("limit=25", "limit=1"));
            uri = new Uri(uri.AbsoluteUri.Replace("offset=", "filter=name:"));
            return await QueryServiceAsync(uri);
        }

        public static async Task<string> ExecuteSearchLimitOneAsync(string query)
        {

            var uri = comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query, new List<string> { "deck", "id", "image", "name", "publisher" });
            uri = new Uri(uri.AbsoluteUri.Replace("limit=25", "limit=1"));
            return await QueryServiceAsync(uri);
        }

        public static async Task<string> GetLatestUpdatedCharacters()
        {
            var uri = comicVineSource.ContructUrl(Resources.ResourcesEnum.Characters, new List<string> { "deck", "id", "image", "name", "publisher" }, "date_last_updated", "asc", 15);
            return await QueryServiceAsync(uri);
        }

        public static async Task<string> GetOffsetCharacterIds(int offset)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Characters, offset.ToString(), "id"));
        }

        #region Get Suggestion Lists

        public static async Task<string> GetSuggestionList(Resources.ResourcesEnum resourceEnum, int offset)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(resourceEnum, offset.ToString(), new List<string> { "name" }));
        }

        #endregion

        #region GetBasic methods

        public static async Task<string> GetCharacterAsync(int characterId)
        {
            List<string> filters = new List<string> { "aliases", "birth", "count_of_issue_appearances", "deck", "description", "first_appeared_in_issue",
                "id", "image", "name", "publisher", "real_name", "teams" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, characterId.ToString(), filters));
        }

        public static async Task<string> GetTeamAsync(int teamId)
        {
            List<string> filters = new List<string> { "aliases", "deck", "description", "first_appeared_in_issue", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId.ToString(), filters));
        }

        public static async Task<string> GetLocationAsync(int locationId)
        {
            List<string> filters = new List<string> { "aliases", "count_of_issue_appearances", "deck", "description", "first_appeared_in_issue", 
                "id", "image", "name", "volume_credits" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Location, locationId.ToString(), filters));
        }

        public static async Task<string> GetPublisherAsync(int publisherId)
        {
            List<string> filters = new List<string> { "aliases", "deck", "description", "id", "image", "name", "location_address", "location_city", "location_state" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Publisher, publisherId.ToString(), filters));
        }

        public static async Task<string> GetConceptAsync(int conceptId)
        {
            List<string> filters = new List<string> { "count_of_isssue_appearances", "deck", "description", "first_appeared_in_issue", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Concept, conceptId.ToString(), filters));
        }

        public static async Task<string> GetCreatorAsync(int personId)
        {
            List<string> filters = new List<string> { "birth", "country", "created_characters", "death", "deck", "description", "hometown", "id",
                "image", "name", "website" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Person, personId.ToString(), filters));
        }

        public static async Task<string> GetMovieAsync(int movieId)
        {
            List<string> filters = new List<string> { "box_office_revenue", "budget", "deck", "description", "id", "image", "name", "rating", "release_date", "runtime", "total_revenue" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Movie, movieId.ToString(), filters));
        }

        public static async Task<string> GetStoryArcAsync(int storyArcId)
        {
            List<string> filters = new List<string> { "deck", "description", "first_appeared_in_issue", "id", "image", "issues", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.StoryArc, storyArcId.ToString(), filters));
        }

        public static async Task<string> GetObjectAsync(int objectId)
        {
            List<string> filters = new List<string> { "aliases", "count_of_issue_appearances", "deck", "description", "first_appeared_in_issue", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Object, objectId.ToString(), filters));
        }

        #endregion

        #region GetFiltered methods

        public static async Task<string> GetFilteredCharacterAsync(int characterId, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, characterId.ToString(), filter));
        }

        public static async Task<string> GetFilteredTeamAsync(int teamId, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId.ToString(), filter));
        }

        public static async Task<string> GetFilteredIssueAsync(int issueId, List<string> filters)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId.ToString(), filters));
        }

        public static async Task<string> GetFilteredMovieAsync(int movieId, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Movie, movieId.ToString(), filter));
        }

        public static async Task<string> GetSpecificIssueAsync(int volumeId, string issueNumber)
        {
            List<string> filters = new List<string> { "cover_date", "description", "id", "image", "issue_number", "name", "store_date", "volume" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issues, volumeId.ToString(), issueNumber, filters));
        }

        public static async Task<string> GetFilteredVolumeAsync(int volumeId, List<string> filters)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Volume, volumeId.ToString(), filters));
        }

        #endregion

        #region GetQuick methods

        public static async Task<string> GetQuickCharacterAsync(int characterId)
        {
            List<string> filters = new List<string> { "deck", "id", "image", "name", "publisher" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, characterId.ToString(), filters));
        }

        public static async Task<string> GetQuickTeamAsync(int teamId)
        {
            List<string> filters = new List<string> { "deck", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId.ToString(), filters));
        }

        public static async Task<string> GetQuickVolumeAsync(int volumeId)
        {
            List<string> filters = new List<string> { "description", "id", "image", "name", "start_year" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Volume, volumeId.ToString(), filters));
        }

        public static async Task<string> GetQuickIssueAsync(int issueId)
        {
            List<string> filters = new List<string> { "cover_date", "description", "id", "image", "issue_number", "name", "store_date", "volume" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId.ToString(), filters));
        }

        public static async Task<string> GetQuickCreatorAsync(int personId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Person, personId.ToString(), filters));
        }

        public static async Task<string> GetQuickLocationAsync(int locationId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Location, locationId.ToString(), filters));
        }

        public static async Task<string> GetQuickConceptAsync(int conceptId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Concept, conceptId.ToString(), filters));
        }

        public static async Task<string> GetQuickObjectAsync(int objectId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Object, objectId.ToString(), filters));
        }

        public static async Task<string> GetQuickStoryArcAsync(int storyArcId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.StoryArc, storyArcId.ToString(), filters));
        }

        #endregion

        #region Format Description

        public static async Task<Description> FormatDescriptionAsync(string descriptionString)
        {
            return await Task.Run(() => new DescriptionMapper().MapDescription(descriptionString));
        }
        public static async Task<Section> FormatDescriptionAsync(Issue issue)
        {
            if (issue.Description != null)
            {
                return await Task.Run(() => new DescriptionMapper().MapDescription(issue));
            }
            return new Section();
        }
        public static async Task<Section> FormatDescriptionAsync(Volume volume)
        {
            if (volume.Description != null)
            {
                return await Task.Run(() => new DescriptionMapper().MapDescription(volume));
            }
            return new Section();
        }

        #endregion

        private static async Task<string> QueryServiceAsync(Uri uri)
        {
            var response = await client.GetAsync(uri, cts.Token);
            string content;
            try
            {
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                content = ServiceConstants.QueryNotFound;
            }

            if (content.Contains(ServiceConstants.ObjectNotFound))
            {
                content = ServiceConstants.QueryNotFound;
            }

            return content;
        }

        #region ConstructUrl

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query)
        {
            string uri = ServiceConstants.ComicVineBaseUrl + Resources.GetResourceTerm(resourcesEnum) + "/";
            switch (resourcesEnum)
            {
                case Resources.ResourcesEnum.Search:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "&limit=25&";
                    break;
                case Resources.ResourcesEnum.Issues:
                    uri += "?";
                    break;
                case Resources.ResourcesEnum.Characters:
                case Resources.ResourcesEnum.Teams:
                case Resources.ResourcesEnum.Volumes:
                    uri += "?offset=" + query + "&";
                    break;
                default:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "/?";
                    break;
            }
            uri += ServiceConstants.ComicVineApiKey + "&" + ServiceConstants.ComicVineFormat;
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query, string filter)
        {
            string uri = this.ContructUrl(resourcesEnum, query).AbsoluteUri;
            uri += "&field_list=" + filter.ToLowerInvariant();
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query, IEnumerable<string> filters)
        {
            string uri = this.ContructUrl(resourcesEnum, query).AbsoluteUri;
            uri += "&field_list=" + string.Join(",", filters).ToLowerInvariant();
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string volumeId, string issueId, IEnumerable<string> filters)
        {
            string uri = this.ContructUrl(resourcesEnum, volumeId).AbsoluteUri;
            uri += "&field_list=" + string.Join(",", filters).ToLowerInvariant();
            uri += "&filter=volume:" + volumeId + ",issue_number:" + issueId;
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, IEnumerable<string> filters, string sortField, string sortDirection, int resultLimit)
        {
            string uri = this.ContructUrl(resourcesEnum, resultLimit.ToString()).AbsoluteUri;
            uri += "&field_list=" + string.Join(",", filters).ToLowerInvariant();
            uri += "&sort=" + sortField + ":" + sortDirection;
            uri = uri.Replace("offset=", "limit=");
            return new Uri(uri);
        }

        #endregion

        #region CancellationTokenSource

        public static void CancelTask()
        {
            cts.Cancel();
        }

        public static void ReinstateCts()
        {
            cts = new CancellationTokenSource();
        }

        public static bool IsCanceled()
        {
            return cts.IsCancellationRequested;
        }

        #endregion
    }
}
